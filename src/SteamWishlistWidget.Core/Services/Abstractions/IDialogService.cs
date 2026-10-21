using SteamWishlistWidget.Models;

namespace SteamWishlistWidget.Services.Abstractions;

/// <summary>Janelas abertas a partir dos ViewModels, sem eles conhecerem as telas.</summary>
public interface IDialogService
{
    /// <summary>Abre as configurações. Retorna true se o usuário salvou.</summary>
    Task<bool> ShowSettingsAsync(AppSettings settings);
}
