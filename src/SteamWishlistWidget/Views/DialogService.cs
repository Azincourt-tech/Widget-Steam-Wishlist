using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using SteamWishlistWidget.Models;
using SteamWishlistWidget.Services.Abstractions;
using SteamWishlistWidget.ViewModels;

namespace SteamWishlistWidget.Views;

/// <summary>
/// Abre as janelas pedidas pelos ViewModels, que assim não dependem das telas.
/// </summary>
public sealed class DialogService(
    ISteamApiClient steam,
    ISettingsStore store,
    IStartupRegistration startup,
    IThemeService themes) : IDialogService
{
    public async Task<bool> ShowSettingsAsync(AppSettings settings)
    {
        var viewModel = new SettingsViewModel(settings, steam, store, startup, themes);
        var window = new SettingsWindow(viewModel);

        var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        return owner is null ? false : await window.ShowDialog<bool>(owner);
    }
}
