namespace SteamWishlistWidget.Services.Abstractions;

/// <summary>Abre a página de um jogo na loja.</summary>
public interface IStoreLauncher
{
    void OpenApp(int appId);
}
