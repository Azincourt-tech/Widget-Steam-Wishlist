#if WINDOWS
using SteamWishlistWidget.Models;
using SteamWishlistWidget.Services;
using SteamWishlistWidget.Services.Abstractions;
using Microsoft.Toolkit.Uwp.Notifications;

namespace SteamWishlistWidget.Platform.Windows;

/// <summary>
/// Notificação nativa do Windows (Central de Notificações; respeita o "Não incomodar"). O pacote registra
/// o widget como remetente de notificações no registro do usuário na primeira vez, com o nome do
/// AssemblyTitle e o ícone do .exe.
/// </summary>
public sealed class WindowsNotificationService : INotificationService
{
    public void NotifySale(GameDeal deal)
    {
        var prices = deal.OriginalPrice is null
            ? $"{deal.DiscountText}: por {deal.FinalPrice}"
            : $"{deal.DiscountText}: de {deal.OriginalPrice} por {deal.FinalPrice}";
        try
        {
            new ToastContentBuilder()
                .AddText($"{deal.Name} entrou em promoção")
                .AddText(prices)
                .Show();
        }
        catch (Exception ex)
        {
            // Sem notificação o widget segue normal: a lista já mostra a promoção.
            ErrorLog.Write("Falha ao mostrar a notificação.", ex);
        }
    }
}
#endif
