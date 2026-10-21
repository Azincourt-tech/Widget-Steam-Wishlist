using System.Text.RegularExpressions;

namespace SteamWishlistWidget.Services;

/// <summary>
/// Entende o que o usuário colou: SteamID64, link /profiles/&lt;id&gt; ou link/nome /id/&lt;nome&gt;.
/// </summary>
public static partial class SteamIdParser
{
    // SteamID64 de contas individuais: 17 dígitos começando com 7656119.
    [GeneratedRegex(@"^7656119\d{10}$")]
    private static partial Regex SteamId64Regex();

    [GeneratedRegex(@"^(?:https?://)?(?:www\.)?steamcommunity\.com/profiles/(\d{17})(?:[/?#].*)?$", RegexOptions.IgnoreCase)]
    private static partial Regex ProfileUrlRegex();

    [GeneratedRegex(@"^(?:https?://)?(?:www\.)?steamcommunity\.com/id/([^/?#]+)(?:[/?#].*)?$", RegexOptions.IgnoreCase)]
    private static partial Regex VanityUrlRegex();

    // Nomes personalizados da Steam: letras, números, _ e -, até 32 caracteres.
    [GeneratedRegex(@"^[A-Za-z0-9_-]{2,32}$")]
    private static partial Regex VanityNameRegex();

    public static bool IsValidSteamId64(string? value) => value is not null && SteamId64Regex().IsMatch(value);

    /// <summary>
    /// Devolve o SteamID64 (quando já está no texto) ou o nome personalizado a ser resolvido online.
    /// </summary>
    public static ParsedProfile Parse(string? input)
    {
        var text = input?.Trim() ?? "";

        if (IsValidSteamId64(text))
            return ParsedProfile.FromSteamId(text);

        var profile = ProfileUrlRegex().Match(text);
        if (profile.Success && IsValidSteamId64(profile.Groups[1].Value))
            return ParsedProfile.FromSteamId(profile.Groups[1].Value);

        var vanity = VanityUrlRegex().Match(text);
        var name = vanity.Success ? vanity.Groups[1].Value : text;
        if (VanityNameRegex().IsMatch(name))
            return ParsedProfile.FromVanity(name);

        return ParsedProfile.Invalid;
    }
}

public readonly record struct ParsedProfile(string? SteamId, string? VanityName)
{
    public static ParsedProfile Invalid => default;
    public static ParsedProfile FromSteamId(string id) => new(id, null);
    public static ParsedProfile FromVanity(string name) => new(null, name);

    public bool IsValid => SteamId is not null || VanityName is not null;
}
