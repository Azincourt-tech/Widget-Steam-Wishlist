using SteamWishlistWidget.Models;
using SteamWishlistWidget.Services.Abstractions;

namespace SteamWishlistWidget.Services.Demo;

/// <summary>
/// Configurações só em memória, para o modo demonstração não alterar o config.json real.
/// </summary>
public sealed class InMemorySettingsStore(AppSettings initial) : ISettingsStore
{
    private AppSettings _current = initial.Clone();

    public AppSettings Load() => _current.Clone();

    public void Save(AppSettings settings) => _current = settings.Clone();
}
