namespace SteamWishlistWidget.Services.Abstractions;

/// <summary>
/// Protege dados sensíveis salvos em disco (hoje, o SteamID), presos à conta do usuário.
/// Cada sistema tem a sua implementação: DPAPI no Windows; no futuro, libsecret no Linux.
/// </summary>
public interface ISecretProtector
{
    string Protect(string plainText);

    /// <summary>Devolve null se o valor estiver corrompido ou tiver sido gerado por outro usuário/máquina.</summary>
    string? TryUnprotect(string? protectedText);
}
