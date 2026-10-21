namespace SteamWishlistWidget.Services.Abstractions;

/// <summary>
/// Temporizador que dispara na thread da interface. Cada framework de interface fornece o seu
/// (DispatcherTimer no WPF; no futuro, o DispatcherTimer do Avalonia).
/// </summary>
public interface IUiTimer
{
    TimeSpan Interval { get; set; }

    bool IsEnabled { get; }

    event EventHandler? Tick;

    void Start();

    void Stop();
}
