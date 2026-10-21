using SteamWishlistWidget.Constants;
using SteamWishlistWidget.Models;
using SteamWishlistWidget.Services.Abstractions;

namespace SteamWishlistWidget.Services.Demo;

/// <summary>
/// Steam de mentira para o modo demonstração (--demo): lista fixa de jogos de exemplo, com preços fictícios.
/// Serve para capturas de tela e para testar a interface sem uma conta Steam.
/// </summary>
public sealed class DemoSteamApiClient : ISteamApiClient
{
    /// <summary>SteamID fictício (formato válido, não pertence a ninguém).</summary>
    public const string DemoSteamId = "76561190000000001";

    private static readonly GameDeal[] Games =
    [
        Game(1174180, "Red Dead Redemption 2", 7497, 29990),
        Game(1091500, "Cyberpunk 2077", 5997, 19990),
        Game(1145360, "Hades", 3699, 7399),
        Game(413150, "Stardew Valley", 1999, 2499),
        Game(105600, "Terraria", 1999),
        Game(367520, "Hollow Knight", 2799),
        Game(646570, "Slay the Spire", 4699),
        Game(292030, "The Witcher 3: Wild Hunt", 12999),
        Game(1086940, "Baldur's Gate 3", 19999),
        Game(1245620, "ELDEN RING", 22990),
    ];

    public Task<string> ResolveSteamIdAsync(string input, CancellationToken ct = default) =>
        Task.FromResult(DemoSteamId);

    public async Task<List<GameDeal>> GetWishlistDealsAsync(AppSettings settings, CancellationToken ct = default)
    {
        // Pequena espera para a mensagem de carregamento aparecer como no uso real.
        await Task.Delay(300, ct);
        return WishlistSorter.Sort(Games, settings.OnlyDiscounted);
    }

    private static GameDeal Game(int appId, string name, long finalCents, long? originalCents = null)
    {
        var discount = originalCents is long original ? (int)Math.Round(100 - finalCents * 100.0 / original) : 0;
        return new GameDeal(
            appId,
            name,
            FormatBrl(finalCents),
            originalCents is long o ? FormatBrl(o) : null,
            discount,
            finalCents,
            $"{SteamEndpoints.AssetBase}steam/apps/{appId}/capsule_231x87.jpg");
    }

    private static string FormatBrl(long cents) => $"R${cents / 100},{cents % 100:00}";
}
