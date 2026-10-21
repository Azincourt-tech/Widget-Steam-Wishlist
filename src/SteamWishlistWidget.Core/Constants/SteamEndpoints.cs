namespace SteamWishlistWidget.Constants;

/// <summary>
/// Endereços das APIs públicas da Steam usadas pelo widget. Todos em HTTPS.
/// </summary>
public static class SteamEndpoints
{
    public const string Wishlist = "https://api.steampowered.com/IWishlistService/GetWishlist/v1/";
    public const string StoreItems = "https://api.steampowered.com/IStoreBrowseService/GetItems/v1/";
    public const string CommunityVanityProfile = "https://steamcommunity.com/id/{0}/?xml=1";
    public const string CommunityProfile = "https://steamcommunity.com/profiles/{0}";
    public const string StoreApp = "https://store.steampowered.com/app/{0}";
    public const string AssetBase = "https://shared.akamai.steamstatic.com/store_item_assets/";

    /// <summary>Únicos hosts de onde o widget aceita carregar imagens.</summary>
    public static readonly IReadOnlySet<string> TrustedImageHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "shared.akamai.steamstatic.com",
        "shared.cloudflare.steamstatic.com",
        "shared.fastly.steamstatic.com",
    };

    /// <summary>Quantos jogos pedir à loja por chamada.</summary>
    public const int StoreItemsBatchSize = 50;
}
