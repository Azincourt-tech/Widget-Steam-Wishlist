using SteamWishlistWidget.Models;

namespace SteamWishlistWidget.Services.Abstractions;

public interface ISteamApiClient
{
    /// <summary>Converte link do perfil, /id/nome ou SteamID64 em SteamID64.</summary>
    Task<string> ResolveSteamIdAsync(string input, CancellationToken ct = default);

    /// <summary>Lista de desejos já com preços e ordenada.</summary>
    Task<List<GameDeal>> GetWishlistDealsAsync(AppSettings settings, CancellationToken ct = default);
}
