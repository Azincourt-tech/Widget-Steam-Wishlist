using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using SteamWishlistWidget.Services.Abstractions;

namespace SteamWishlistWidget.Services.Security;

/// <summary>
/// Criptografa dados com a DPAPI do Windows, presa à conta do usuário atual:
/// o valor salvo só pode ser lido por este usuário, nesta máquina.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsDataProtector : ISecretProtector
{
    // Entropia extra específica do app, para outro programa não reaproveitar o mesmo blob.
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("SteamWishlistWidget.v1");

    public string Protect(string plainText)
    {
        var data = ProtectedData.Protect(Encoding.UTF8.GetBytes(plainText), Entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(data);
    }

    public string? TryUnprotect(string? protectedText)
    {
        if (string.IsNullOrWhiteSpace(protectedText))
            return null;

        try
        {
            var data = ProtectedData.Unprotect(Convert.FromBase64String(protectedText), Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(data);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            return null;
        }
    }
}
