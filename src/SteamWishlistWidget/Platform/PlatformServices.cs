using SteamWishlistWidget.Platform.Windows;
using SteamWishlistWidget.Services.Abstractions;
using SteamWishlistWidget.Services.Security;

namespace SteamWishlistWidget.Platform;

/// <summary>
/// Escolhe a implementação de cada serviço que depende do sistema operacional.
/// Hoje só o Windows é suportado; o Linux está no roadmap (docs/ROADMAP.md). O macOS não será suportado.
/// </summary>
public static class PlatformServices
{
    private const string NotSupportedMessage =
        "Por enquanto o widget só roda no Windows. O suporte ao Linux está planejado (docs/ROADMAP.md). " +
        "Para testar a interface em outro sistema, use o modo demonstração: --demo.";

    public static ISecretProtector CreateSecretProtector() =>
        OperatingSystem.IsWindows()
            ? new WindowsDataProtector()
            : throw new PlatformNotSupportedException(NotSupportedMessage);

    public static IStartupRegistration CreateStartupRegistration() =>
        OperatingSystem.IsWindows()
            ? new WindowsStartupRegistration()
            : throw new PlatformNotSupportedException(NotSupportedMessage);
}
