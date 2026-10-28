using SteamWishlistWidget.Models;
using SteamWishlistWidget.Services.Abstractions;

namespace SteamWishlistWidget.Services;

/// <summary>Não avisa nada: usado no modo demonstração e em sistemas sem notificação implementada.</summary>
public sealed class NoNotificationService : INotificationService
{
    public void NotifySale(GameDeal deal)
    {
    }
}
