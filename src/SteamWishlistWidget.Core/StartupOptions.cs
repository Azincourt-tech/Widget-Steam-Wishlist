using SteamWishlistWidget.Models;

namespace SteamWishlistWidget;

/// <summary>
/// Opções de linha de comando.
/// <list type="bullet">
/// <item><c>--demo</c>: abre com jogos e perfil fictícios, sem tocar nas configurações reais.</item>
/// <item><c>--theme=Dark|Light|Glass</c>: tema inicial do modo demonstração.</item>
/// </list>
/// </summary>
public sealed record StartupOptions(bool Demo, string? Theme)
{
    public static StartupOptions Parse(IEnumerable<string> args)
    {
        var demo = false;
        string? theme = null;

        foreach (var arg in args)
        {
            if (arg.Equals("--demo", StringComparison.OrdinalIgnoreCase))
                demo = true;
            else if (arg.StartsWith("--theme=", StringComparison.OrdinalIgnoreCase))
                theme = ThemeNames.Normalize(arg["--theme=".Length..]);
        }

        return new StartupOptions(demo, theme);
    }
}
