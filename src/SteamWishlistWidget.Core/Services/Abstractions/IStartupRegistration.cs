namespace SteamWishlistWidget.Services.Abstractions;

/// <summary>Iniciar junto com o Windows.</summary>
public interface IStartupRegistration
{
    bool IsEnabled { get; }

    void Set(bool enabled);
}
