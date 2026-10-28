using SteamWishlistWidget.Models;
using SteamWishlistWidget.Services.Abstractions;

namespace SteamWishlistWidget.Tests.Fakes;

/// <summary>Steam falsa: devolve o que o teste mandar, sem internet.</summary>
internal sealed class FakeSteamApiClient : ISteamApiClient
{
    public List<GameDeal> Deals { get; set; } = [];
    public Exception? WishlistError { get; set; }
    public Func<string, string> Resolve { get; set; } = input => input;
    public int WishlistCalls { get; private set; }

    public Task<string> ResolveSteamIdAsync(string input, CancellationToken ct = default) =>
        Task.FromResult(Resolve(input));

    public Task<List<GameDeal>> GetWishlistDealsAsync(AppSettings settings, CancellationToken ct = default)
    {
        WishlistCalls++;
        return WishlistError is null ? Task.FromResult(Deals.ToList()) : Task.FromException<List<GameDeal>>(WishlistError);
    }
}

internal sealed class FakeSettingsStore : ISettingsStore
{
    public AppSettings? LastSaved { get; private set; }
    public int SaveCount { get; private set; }

    public AppSettings Load() => LastSaved?.Clone() ?? new AppSettings();

    public void Save(AppSettings settings)
    {
        LastSaved = settings.Clone();
        SaveCount++;
    }
}

internal sealed class FakeStoreLauncher : IStoreLauncher
{
    public List<int> Opened { get; } = [];

    public void OpenApp(int appId) => Opened.Add(appId);
}

internal sealed class FakeStartupRegistration : IStartupRegistration
{
    public bool IsEnabled { get; private set; }

    public void Set(bool enabled) => IsEnabled = enabled;
}

internal sealed class FakeThemeService : IThemeService
{
    public List<string> Applied { get; } = [];

    public void Apply(string theme) => Applied.Add(theme);
}

internal sealed class FakeDialogService : IDialogService
{
    /// <summary>O que o "usuário" faz na tela de configurações.</summary>
    public Func<AppSettings, bool> OnShowSettings { get; set; } = _ => false;

    public Task<bool> ShowSettingsAsync(AppSettings settings) => Task.FromResult(OnShowSettings(settings));
}

/// <summary>Temporizador que nunca dispara sozinho; o teste controla.</summary>
internal sealed class FakeUiTimer : IUiTimer
{
    public TimeSpan Interval { get; set; }
    public bool IsEnabled { get; private set; }
    public event EventHandler? Tick;

    public void Start() => IsEnabled = true;
    public void Stop() => IsEnabled = false;
    public void Fire() => Tick?.Invoke(this, EventArgs.Empty);
}

/// <summary>"Criptografia" reversível e previsível, para testar a gravação em qualquer sistema.</summary>
internal sealed class FakeSecretProtector : ISecretProtector
{
    private const string Prefix = "protegido:";

    public string Protect(string plainText) =>
        Prefix + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(new string(plainText.Reverse().ToArray())));

    public string? TryUnprotect(string? protectedText)
    {
        if (protectedText is null || !protectedText.StartsWith(Prefix, StringComparison.Ordinal))
            return null;
        try
        {
            var reversed = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(protectedText[Prefix.Length..]));
            return new string(reversed.Reverse().ToArray());
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
internal sealed class FakeNotificationService : INotificationService
{
    public List<GameDeal> Notified { get; } = [];

    public void NotifySale(GameDeal deal) => Notified.Add(deal);
}
