using System.Diagnostics;
using SteamWishlistWidget.Constants;
using SteamWishlistWidget.Services.Abstractions;

namespace SteamWishlistWidget.Services;

/// <summary>
/// Abre a página do jogo na loja. A URL é montada aqui a partir do AppID numérico,
/// nunca de texto vindo da internet, então só abre endereços da própria Steam.
/// </summary>
public sealed class StoreLauncher : IStoreLauncher
{
    public void OpenApp(int appId)
    {
        if (appId <= 0)
            return;

        var url = string.Format(SteamEndpoints.StoreApp, appId);
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
