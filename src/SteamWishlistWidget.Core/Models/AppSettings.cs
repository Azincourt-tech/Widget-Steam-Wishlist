using System.Text.RegularExpressions;
using SteamWishlistWidget.Services;

namespace SteamWishlistWidget.Models;

/// <summary>
/// Preferências do usuário. Use <see cref="Normalize"/> depois de ler de fora (arquivo ou tela)
/// para garantir que todos os valores estão dentro do permitido.
/// </summary>
public partial class AppSettings
{
    public static readonly IReadOnlyList<string> Languages = ["brazilian", "english", "latam"];
    public static readonly IReadOnlyList<int> VisibleRowsOptions = [3, 5, 8, 10, 15];
    public static readonly IReadOnlyList<int> RefreshMinutesOptions = [15, 30, 60, 180];

    public const string DefaultCountry = "BR";
    public const string DefaultLanguage = "brazilian";
    public const int DefaultVisibleRows = 5;
    public const int DefaultRefreshMinutes = 60;

    public string? SteamId { get; set; }
    public string Theme { get; set; } = ThemeNames.Glass;
    public string CountryCode { get; set; } = DefaultCountry;
    public string Language { get; set; } = DefaultLanguage;
    public int VisibleRows { get; set; } = DefaultVisibleRows;
    public int RefreshMinutes { get; set; } = DefaultRefreshMinutes;
    public bool OnlyDiscounted { get; set; }
    public bool Pinned { get; set; }
    public bool Collapsed { get; set; }
    public double? Left { get; set; }
    public double? Top { get; set; }

    [GeneratedRegex("^[A-Z]{2}$")]
    private static partial Regex CountryCodeRegex();

    public static bool IsValidCountryCode(string? value) => value is not null && CountryCodeRegex().IsMatch(value);

    /// <summary>
    /// Corrige qualquer valor fora do esperado (arquivo editado à mão, versão antiga, etc.).
    /// </summary>
    public AppSettings Normalize()
    {
        if (!SteamIdParser.IsValidSteamId64(SteamId))
            SteamId = null;

        Theme = ThemeNames.Normalize(Theme);
        CountryCode = CountryCode?.Trim().ToUpperInvariant() ?? "";
        if (!IsValidCountryCode(CountryCode))
            CountryCode = DefaultCountry;
        if (!Languages.Contains(Language))
            Language = DefaultLanguage;
        if (!VisibleRowsOptions.Contains(VisibleRows))
            VisibleRows = DefaultVisibleRows;
        if (!RefreshMinutesOptions.Contains(RefreshMinutes))
            RefreshMinutes = DefaultRefreshMinutes;
        if (Left is double left && !double.IsFinite(left))
            Left = null;
        if (Top is double top && !double.IsFinite(top))
            Top = null;
        return this;
    }

    public AppSettings Clone() => (AppSettings)MemberwiseClone();
}
