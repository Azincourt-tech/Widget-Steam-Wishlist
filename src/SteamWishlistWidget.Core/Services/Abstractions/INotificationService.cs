using SteamWishlistWidget.Models;

namespace SteamWishlistWidget.Services.Abstractions;

/// <summary>Mostra um aviso do sistema quando um jogo da lista entra em promoção.</summary>
public interface INotificationService
{
    void NotifySale(GameDeal deal);
}
