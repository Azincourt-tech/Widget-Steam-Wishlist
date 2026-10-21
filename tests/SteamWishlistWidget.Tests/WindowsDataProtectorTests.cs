using SteamWishlistWidget.Services.Security;

namespace SteamWishlistWidget.Tests;

/// <summary>
/// Testes da criptografia real do Windows (DPAPI). No Linux eles não têm o que testar e terminam sem verificar nada.
/// </summary>
public class WindowsDataProtectorTests
{
    private const string SteamId = "76561190000000001";

    [Fact]
    public void RoundTrip_AndRejectsTamperedValue()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var protector = new WindowsDataProtector();
        var protectedValue = protector.Protect(SteamId);
        var tampered = protectedValue[..^4] + "AAAA";

        Assert.DoesNotContain(SteamId, protectedValue);
        Assert.Equal(SteamId, protector.TryUnprotect(protectedValue));
        Assert.Null(protector.TryUnprotect(tampered));
        Assert.Null(protector.TryUnprotect("isto não é base64"));
        Assert.Null(protector.TryUnprotect(null));
    }
}
