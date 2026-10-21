using SteamWishlistWidget.Models;

namespace SteamWishlistWidget.Services.Abstractions;

public interface ISettingsStore
{
    AppSettings Load();

    void Save(AppSettings settings);
}
