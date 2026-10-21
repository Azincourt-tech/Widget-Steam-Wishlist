using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamWishlistWidget.Constants;
using SteamWishlistWidget.Models;
using SteamWishlistWidget.Services;
using SteamWishlistWidget.Services.Abstractions;

namespace SteamWishlistWidget.ViewModels;

/// <summary>
/// Tela de configurações. Edita uma cópia dos valores e só grava nas configurações
/// recebidas quando o usuário salva e tudo foi validado.
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettings _settings;
    private readonly ISteamApiClient _steam;
    private readonly ISettingsStore _store;
    private readonly IStartupRegistration _startup;
    private readonly IThemeService _themes;
    private readonly string _initialProfileLink;
    private readonly bool _initialized;

    public SettingsViewModel(
        AppSettings settings,
        ISteamApiClient steam,
        ISettingsStore store,
        IStartupRegistration startup,
        IThemeService themes)
    {
        _settings = settings;
        _steam = steam;
        _store = store;
        _startup = startup;
        _themes = themes;

        _initialProfileLink = string.IsNullOrEmpty(settings.SteamId)
            ? ""
            : string.Format(SteamEndpoints.CommunityProfile, settings.SteamId);

        Theme = ThemeNames.Normalize(settings.Theme);
        ProfileInput = _initialProfileLink;
        Language = settings.Language;
        CountryCode = settings.CountryCode;
        VisibleRows = settings.VisibleRows;
        RefreshMinutes = settings.RefreshMinutes;
        OnlyDiscounted = settings.OnlyDiscounted;
        StartWithWindows = startup.IsEnabled;
        _initialized = true;
    }

    [ObservableProperty]
    private string _theme = ThemeNames.Glass;

    [ObservableProperty]
    private string _profileInput = "";

    [ObservableProperty]
    private string _language = AppSettings.DefaultLanguage;

    [ObservableProperty]
    private string _countryCode = AppSettings.DefaultCountry;

    [ObservableProperty]
    private int _visibleRows = AppSettings.DefaultVisibleRows;

    [ObservableProperty]
    private int _refreshMinutes = AppSettings.DefaultRefreshMinutes;

    [ObservableProperty]
    private bool _onlyDiscounted;

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>A View fecha a janela com o resultado (true = salvou).</summary>
    public event EventHandler<bool>? CloseRequested;

    // Prévia ao vivo: o tema muda na hora; quem abriu a tela reaplica o tema salvo ao fechar.
    partial void OnThemeChanged(string value)
    {
        if (_initialized)
            _themes.Apply(value);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;

        var country = CountryCode.Trim().ToUpperInvariant();
        if (!AppSettings.IsValidCountryCode(country))
        {
            ErrorMessage = "País deve ter 2 letras, por exemplo BR ou US.";
            return;
        }

        var steamId = _settings.SteamId;
        var profile = ProfileInput.Trim();
        if (profile.Length > 0 && profile != _initialProfileLink)
        {
            try
            {
                steamId = await _steam.ResolveSteamIdAsync(profile);
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
                return;
            }
            catch (Exception ex)
            {
                ErrorLog.Write("Falha ao validar o perfil nas configurações.", ex);
                ErrorMessage = "Não foi possível falar com a Steam para validar o perfil. Tente de novo.";
                return;
            }
        }

        var updated = _settings.Clone();
        updated.Theme = Theme;
        updated.SteamId = steamId;
        updated.Language = Language;
        updated.CountryCode = country;
        updated.VisibleRows = VisibleRows;
        updated.RefreshMinutes = RefreshMinutes;
        updated.OnlyDiscounted = OnlyDiscounted;
        updated.Normalize();

        try
        {
            _store.Save(updated);
            _startup.Set(StartWithWindows);
        }
        catch (Exception ex)
        {
            ErrorLog.Write("Falha ao salvar as configurações.", ex);
            ErrorMessage = "Não foi possível salvar as configurações.";
            return;
        }

        CopyTo(updated, _settings);
        CloseRequested?.Invoke(this, true);
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(this, false);

    private static void CopyTo(AppSettings source, AppSettings target)
    {
        target.Theme = source.Theme;
        target.SteamId = source.SteamId;
        target.Language = source.Language;
        target.CountryCode = source.CountryCode;
        target.VisibleRows = source.VisibleRows;
        target.RefreshMinutes = source.RefreshMinutes;
        target.OnlyDiscounted = source.OnlyDiscounted;
    }
}
