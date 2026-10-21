using SteamWishlistWidget.Services.Abstractions;

namespace SteamWishlistWidget.Themes;

public sealed class ThemeService : IThemeService
{
    public void Apply(string theme) => ThemeManager.Apply(theme);
}
