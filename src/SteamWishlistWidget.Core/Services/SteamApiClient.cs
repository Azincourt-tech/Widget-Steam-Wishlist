using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Xml;
using SteamWishlistWidget.Constants;
using SteamWishlistWidget.Models;
using SteamWishlistWidget.Services.Abstractions;

namespace SteamWishlistWidget.Services;

/// <summary>
/// Acesso às APIs públicas da Steam. Não usa API key nem envia nada além do SteamID.
/// A leitura das respostas fica em métodos estáticos para poder ser testada sem internet.
/// </summary>
public sealed class SteamApiClient : ISteamApiClient
{
    private static readonly HttpClient Http = CreateHttpClient();

    private static HttpClient CreateHttpClient()
    {
        var http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20),
            // Limita o tamanho das respostas aceitas (proteção contra respostas inesperadas/gigantes).
            MaxResponseContentBufferSize = 8 * 1024 * 1024,
        };
        http.DefaultRequestHeaders.UserAgent.ParseAdd($"{AppConstants.AppName}/1.0");
        return http;
    }

    /// <summary>
    /// Converte o que o usuário digitou em SteamID64, consultando a Steam só para links /id/nome.
    /// </summary>
    public async Task<string> ResolveSteamIdAsync(string input, CancellationToken ct = default)
    {
        var parsed = SteamIdParser.Parse(input);
        if (parsed.SteamId is not null)
            return parsed.SteamId;
        if (parsed.VanityName is null)
            throw new InvalidOperationException("Link ou SteamID inválido. Use o link do perfil Steam ou o SteamID64.");

        var url = string.Format(SteamEndpoints.CommunityVanityProfile, Uri.EscapeDataString(parsed.VanityName));
        var xml = await Http.GetStringAsync(url, ct);

        var id = ParseSteamIdFromProfileXml(xml);
        if (!SteamIdParser.IsValidSteamId64(id))
            throw new InvalidOperationException("Perfil não encontrado. Confira o link ou o SteamID.");
        return id!;
    }

    public async Task<List<GameDeal>> GetWishlistDealsAsync(AppSettings settings, CancellationToken ct = default)
    {
        if (!SteamIdParser.IsValidSteamId64(settings.SteamId))
            throw new InvalidOperationException("Perfil Steam não configurado.");

        var wishlistJson = await Http.GetStringAsync($"{SteamEndpoints.Wishlist}?steamid={settings.SteamId}", ct);
        var appIds = ParseWishlistAppIds(wishlistJson);
        if (appIds.Count == 0)
            throw new InvalidOperationException(
                "Lista de desejos vazia ou privada. Deixe os 'Detalhes do jogo' do perfil como Público na Steam.");

        var deals = new List<GameDeal>();
        foreach (var chunk in appIds.Chunk(SteamEndpoints.StoreItemsBatchSize))
        {
            var storeJson = await Http.GetStringAsync(BuildStoreItemsUrl(chunk, settings), ct);
            deals.AddRange(ParseStoreItems(storeJson));
        }

        return WishlistSorter.Sort(deals, settings.OnlyDiscounted);
    }

    private static string BuildStoreItemsUrl(IEnumerable<int> appIds, AppSettings settings)
    {
        var input = new
        {
            ids = appIds.Select(id => new { appid = id }),
            context = new { language = settings.Language, country_code = settings.CountryCode },
            data_request = new { include_assets = true },
        };
        return $"{SteamEndpoints.StoreItems}?input_json={Uri.EscapeDataString(JsonSerializer.Serialize(input))}";
    }

    // ---- Leitura das respostas ----

    /// <summary>
    /// Lê o SteamID64 do XML do perfil, sem processar DTD nem recursos externos (evita XXE).
    /// </summary>
    public static string? ParseSteamIdFromProfileXml(string xml)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = 2 * 1024 * 1024,
        };

        try
        {
            using var reader = XmlReader.Create(new StringReader(xml), settings);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "steamID64")
                    return reader.ReadElementContentAsString().Trim();
            }
        }
        catch (XmlException)
        {
            // Resposta não é o XML esperado (perfil inexistente, página de erro...).
        }
        return null;
    }

    public static List<int> ParseWishlistAppIds(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("response", out var response)
            || !response.TryGetProperty("items", out var items)
            || items.ValueKind != JsonValueKind.Array)
            return [];

        return items.EnumerateArray()
            .Where(i => i.TryGetProperty("appid", out var id) && id.ValueKind == JsonValueKind.Number)
            .Select(i => i.GetProperty("appid").GetInt32())
            .Where(id => id > 0)
            .Distinct()
            .ToList();
    }

    public static List<GameDeal> ParseStoreItems(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var result = new List<GameDeal>();

        if (!doc.RootElement.TryGetProperty("response", out var response)
            || !response.TryGetProperty("store_items", out var storeItems)
            || storeItems.ValueKind != JsonValueKind.Array)
            return result;

        foreach (var item in storeItems.EnumerateArray())
        {
            // Sem preço = jogo não lançado, gratuito ou indisponível na região.
            if (!item.TryGetProperty("best_purchase_option", out var price)
                || !item.TryGetProperty("appid", out var appIdElement))
                continue;

            var finalPrice = GetString(price, "formatted_final_price");
            if (finalPrice is null)
                continue;

            long.TryParse(GetString(price, "final_price_in_cents"), out var finalCents);
            var discount = price.TryGetProperty("discount_pct", out var pct) && pct.ValueKind == JsonValueKind.Number
                ? Math.Clamp(pct.GetInt32(), 0, 100)
                : 0;

            result.Add(new GameDeal(
                AppId: appIdElement.GetInt32(),
                Name: GetString(item, "name") ?? "",
                FinalPrice: finalPrice,
                OriginalPrice: GetString(price, "formatted_original_price"),
                DiscountPct: discount,
                FinalCents: finalCents,
                ImageUrl: BuildImageUrl(item)));
        }

        return result;
    }

    /// <summary>
    /// Monta a URL da capa e só aceita se apontar para os servidores de imagem da Steam.
    /// </summary>
    public static string? BuildImageUrl(JsonElement item)
    {
        if (!item.TryGetProperty("assets", out var assets))
            return null;

        var format = GetString(assets, "asset_url_format");
        var file = GetString(assets, "small_capsule");
        if (format is null || file is null)
            return null;

        var url = SteamEndpoints.AssetBase + format.Replace("${FILENAME}", file);
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && uri.Scheme == Uri.UriSchemeHttps
               && SteamEndpoints.TrustedImageHosts.Contains(uri.Host)
            ? uri.AbsoluteUri
            : null;
    }

    private static string? GetString(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
