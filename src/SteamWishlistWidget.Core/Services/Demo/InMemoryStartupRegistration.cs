using SteamWishlistWidget.Services.Abstractions;

namespace SteamWishlistWidget.Services.Demo;

/// <summary>
/// "Iniciar com o Windows" de mentira, para o modo demonstração não mexer no registro.
/// </summary>
public sealed class InMemoryStartupRegistration : IStartupRegistration
{
    public bool IsEnabled { get; private set; }

    public void Set(bool enabled) => IsEnabled = enabled;
}
