using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using SteamWishlistWidget.Constants;
using SteamWishlistWidget.Services;

namespace SteamWishlistWidget.Controls;

/// <summary>
/// Carrega imagens da internet num <see cref="Image"/> (o Avalonia não faz isso sozinho).
/// Uso no XAML: <c>&lt;Image ctl:RemoteImage.Url="{Binding ImageUrl}" /&gt;</c>.
/// Só aceita HTTPS dos servidores de imagem da Steam e guarda as imagens em memória.
/// </summary>
public static class RemoteImage
{
    public static readonly AttachedProperty<string?> UrlProperty =
        AvaloniaProperty.RegisterAttached<Image, string?>("Url", typeof(RemoteImage));

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(20), MaxResponseContentBufferSize = 4 * 1024 * 1024 };
    private static readonly ConcurrentDictionary<string, Task<Bitmap?>> Cache = new();

    static RemoteImage() => UrlProperty.Changed.AddClassHandler<Image>(OnUrlChanged);

    public static string? GetUrl(Image image) => image.GetValue(UrlProperty);

    public static void SetUrl(Image image, string? value) => image.SetValue(UrlProperty, value);

    private static async void OnUrlChanged(Image image, AvaloniaPropertyChangedEventArgs e)
    {
        image.Source = null;
        if (e.NewValue is not string url || !IsTrusted(url))
            return;

        try
        {
            var bitmap = await Cache.GetOrAdd(url, Download);
            // A linha pode ter sido reaproveitada para outro jogo enquanto a imagem baixava.
            if (GetUrl(image) == url)
                image.Source = bitmap;
        }
        catch (Exception ex)
        {
            ErrorLog.Write("Falha ao exibir a capa de um jogo.", ex);
        }
    }

    private static async Task<Bitmap?> Download(string url)
    {
        try
        {
            var bytes = await Http.GetByteArrayAsync(url);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or ArgumentException or IOException)
        {
            // Sem capa: o espaço fica com o fundo neutro.
            Cache.TryRemove(url, out _);
            return null;
        }
    }

    private static bool IsTrusted(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && SteamEndpoints.TrustedImageHosts.Contains(uri.Host);
}
