using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamWishlistWidget.Models;
using SteamWishlistWidget.Services;
using SteamWishlistWidget.Services.Abstractions;

namespace SteamWishlistWidget.ViewModels;

/// <summary>
/// Estado e ações do widget: lista de jogos, mensagens, fixar, minimizar, configurações.
/// Não conhece a janela; a View reage às propriedades (tema, minimizado) para os efeitos visuais.
/// </summary>
public sealed partial class WidgetViewModel : ObservableObject
{
    /// <summary>Altura de cada linha da lista, usada para mostrar N jogos antes da rolagem.</summary>
    public const double RowHeight = 38;

    private readonly AppSettings _settings;
    private readonly ISteamApiClient _steam;
    private readonly ISettingsStore _store;
    private readonly IStoreLauncher _launcher;
    private readonly IThemeService _themes;
    private readonly IDialogService _dialogs;
    private readonly INotificationService _notifications;
    private readonly IUiTimer _timer;
    private readonly SaleAlertTracker _saleAlerts = new();
    private readonly CancellationTokenSource _lifetime = new();
    private readonly bool _initialized;
    private bool _loading;

    public WidgetViewModel(
        AppSettings settings,
        ISteamApiClient steam,
        ISettingsStore store,
        IStoreLauncher launcher,
        IThemeService themes,
        IDialogService dialogs,
        INotificationService notifications,
        IUiTimer timer)
    {
        _timer = timer;
        _notifications = notifications;
        _settings = settings;
        _steam = steam;
        _store = store;
        _launcher = launcher;
        _themes = themes;
        _dialogs = dialogs;

        Theme = ThemeNames.Normalize(settings.Theme);
        IsPinned = settings.Pinned;
        IsCollapsed = settings.Collapsed;
        ApplyListSettings();

        _timer.Tick += async (_, _) => await RefreshAsync();
        _initialized = true;
    }

    // ---- Estado exibido ----

    [ObservableProperty]
    private IReadOnlyList<GameDeal> _deals = [];

    [ObservableProperty]
    private string _subtitle = "Steam";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatus))]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isSetupVisible;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _profileInput = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSaleBadge))]
    private int _onSaleCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PinToolTip))]
    private bool _isPinned;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CollapseMenuHeader))]
    private bool _isCollapsed;

    [ObservableProperty]
    private string _theme = ThemeNames.Glass;

    [ObservableProperty]
    private double _listMaxHeight;

    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);
    public bool HasSaleBadge => OnSaleCount > 0;
    public string PinToolTip => IsPinned ? "Desafixar" : "Fixar (sempre visível)";
    public string CollapseMenuHeader => IsCollapsed ? "Abrir lista" : "Minimizar para ícone";

    /// <summary>Posição salva da janela (null na primeira execução).</summary>
    public double? SavedLeft => _settings.Left;
    public double? SavedTop => _settings.Top;

    /// <summary>A View fecha a janela quando recebe este evento.</summary>
    public event EventHandler? CloseRequested;

    // ---- Ciclo de vida ----

    public async Task InitializeAsync()
    {
        if (string.IsNullOrEmpty(_settings.SteamId))
        {
            IsCollapsed = false;
            ShowSetup();
            return;
        }

        _timer.Start();
        await RefreshAsync();
    }

    public void SaveWindowPosition(double left, double top)
    {
        _settings.Left = left;
        _settings.Top = top;
        SaveSettings();
    }

    public void Shutdown()
    {
        _lifetime.Cancel();
        _timer.Stop();
    }

    // ---- Comandos ----

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (_loading || string.IsNullOrEmpty(_settings.SteamId))
            return;

        _loading = true;
        StatusMessage = "Carregando lista de desejos...";
        try
        {
            var deals = await _steam.GetWishlistDealsAsync(_settings, _lifetime.Token);
            Deals = deals;
            OnSaleCount = deals.Count(d => d.IsDiscounted);
            Subtitle = $"{OnSaleCount} de {deals.Count} em promoção · {DateTime.Now:HH:mm}";
            StatusMessage = deals.Count == 0 ? "Nenhum jogo da lista está em promoção agora." : null;
            NotifyNewSales(deals);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            // Widget fechando.
        }
        catch (Exception ex)
        {
            StatusMessage = $"Não foi possível carregar: {FriendlyError(ex)}";
        }
        finally
        {
            _loading = false;
        }
    }

    private bool CanConnect() => !string.IsNullOrWhiteSpace(ProfileInput);

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        StatusMessage = "Procurando perfil...";
        try
        {
            _settings.SteamId = await _steam.ResolveSteamIdAsync(ProfileInput, _lifetime.Token);
            SaveSettings();
            _saleAlerts.Reset();
            IsSetupVisible = false;
            _timer.Start();
            await RefreshAsync();
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !_lifetime.IsCancellationRequested)
        {
            StatusMessage = FriendlyError(ex);
        }
    }

    [RelayCommand]
    private void OpenDeal(GameDeal? deal)
    {
        if (deal is not null)
            _launcher.OpenApp(deal.AppId);
    }

    [RelayCommand]
    private void TogglePin() => IsPinned = !IsPinned;

    [RelayCommand]
    private void ToggleCollapsed() => IsCollapsed = !IsCollapsed;

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        var previous = (_settings.SteamId, _settings.Language, _settings.CountryCode, _settings.OnlyDiscounted);

        var saved = await _dialogs.ShowSettingsAsync(_settings);

        // Mesmo cancelando, reaplica o tema (a tela de configurações mostra prévia ao vivo).
        _themes.Apply(_settings.Theme);
        Theme = ThemeNames.Normalize(_settings.Theme);
        if (!saved)
            return;

        ApplyListSettings();
        if (string.IsNullOrEmpty(_settings.SteamId))
            return;

        if (IsSetupVisible)
        {
            IsSetupVisible = false;
            _timer.Start();
        }

        var current = (_settings.SteamId, _settings.Language, _settings.CountryCode, _settings.OnlyDiscounted);

        // Outro perfil ou outra loja: a próxima leitura vira a nova base, sem avisar o que já estava em promoção.
        if ((current.SteamId, current.Language, current.CountryCode) != (previous.SteamId, previous.Language, previous.CountryCode))
            _saleAlerts.Reset();

        if (current != previous || Deals.Count == 0)
            await RefreshAsync();
    }

    [RelayCommand]
    private void Close() => CloseRequested?.Invoke(this, EventArgs.Empty);

    // ---- Reações a mudanças ----

    partial void OnIsPinnedChanged(bool value)
    {
        if (!_initialized)
            return;
        _settings.Pinned = value;
        SaveSettings();
    }

    partial void OnIsCollapsedChanged(bool value)
    {
        if (!_initialized)
            return;
        _settings.Collapsed = value;
        SaveSettings();
    }

    partial void OnThemeChanged(string value)
    {
        if (_initialized)
            _themes.Apply(value);
    }

    // ---- Auxiliares ----

    private void ShowSetup()
    {
        Deals = [];
        Subtitle = "Conecte seu perfil";
        OnSaleCount = 0;
        StatusMessage = null;
        IsSetupVisible = true;
    }

    private void ApplyListSettings()
    {
        ListMaxHeight = Math.Max(1, _settings.VisibleRows) * RowHeight;

        _timer.Interval = TimeSpan.FromMinutes(Math.Max(5, _settings.RefreshMinutes));
        if (_timer.IsEnabled)
        {
            // Reinicia a contagem a partir de agora com o novo intervalo.
            _timer.Stop();
            _timer.Start();
        }
    }

    /// <summary>
    /// Avisa os jogos que entraram em promoção desde a leitura anterior. A base é atualizada
    /// mesmo com os avisos desligados, para não avisar de uma vez tudo o que mudou ao religar.
    /// </summary>
    private void NotifyNewSales(IReadOnlyList<GameDeal> deals)
    {
        var entered = _saleAlerts.Update(deals);
        if (!_settings.Notifications)
            return;

        foreach (var deal in entered)
            _notifications.NotifySale(deal);
    }

    private void SaveSettings()
    {
        try
        {
            _store.Save(_settings);
        }
        catch (Exception ex)
        {
            ErrorLog.Write("Falha ao salvar as configurações.", ex);
        }
    }

    /// <summary>
    /// Mensagem curta para o usuário; o detalhe técnico vai só para o log local.
    /// </summary>
    private static string FriendlyError(Exception ex)
    {
        switch (ex)
        {
            case InvalidOperationException:
                return ex.Message;
            case HttpRequestException or TaskCanceledException:
                ErrorLog.Write("Falha de rede ao falar com a Steam.", ex);
                return "sem conexão com a Steam. Tente de novo em instantes.";
            default:
                ErrorLog.Write("Erro inesperado ao carregar a lista.", ex);
                return "erro inesperado (detalhes no log).";
        }
    }
}
