using System.IO;
using System.Text;
using SteamWishlistWidget.Constants;

namespace SteamWishlistWidget.Services;

/// <summary>
/// Log de erros local (%LocalAppData%\SteamWishlistWidget\logs). Não grava SteamID nem dados do perfil.
/// </summary>
public static class ErrorLog
{
    private const long MaxSize = 512 * 1024;
    private static readonly object Gate = new();

    /// <summary>Pasta do log. Os testes trocam para uma pasta temporária.</summary>
    internal static string Folder { get; set; } = AppConstants.LogsFolder;

    public static string FilePath => Path.Combine(Folder, "error.log");

    public static void Write(string message, Exception? ex = null)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(Folder);

                // Mantém o arquivo pequeno: ao passar do limite, guarda só a versão anterior.
                var file = new FileInfo(FilePath);
                if (file.Exists && file.Length > MaxSize)
                    File.Move(FilePath, FilePath + ".old", overwrite: true);

                var entry = new StringBuilder()
                    .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("  ").AppendLine(message);
                if (ex is not null)
                    entry.AppendLine(ex.ToString());

                File.AppendAllText(FilePath, entry.ToString());
            }
        }
        catch
        {
            // Falha ao registrar o erro não pode derrubar o app.
        }
    }
}
