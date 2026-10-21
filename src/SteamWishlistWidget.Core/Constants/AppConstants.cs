using System.IO;

namespace SteamWishlistWidget.Constants;

/// <summary>
/// Nomes e caminhos fixos do aplicativo.
/// </summary>
public static class AppConstants
{
    public const string AppName = "SteamWishlistWidget";
    public const string SingleInstanceMutex = "SteamWishlistWidget.SingleInstance";

    /// <summary>Configurações do usuário (perfil, tema, posição...).</summary>
    public static string SettingsFolder { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);

    /// <summary>Logs de erro, só nesta máquina.</summary>
    public static string LogsFolder { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName, "logs");

    public static string SettingsFile => Path.Combine(SettingsFolder, "config.json");
}
