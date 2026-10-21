using System.IO;
using SteamWishlistWidget.Models;
using SteamWishlistWidget.Services;
using SteamWishlistWidget.Tests.Fakes;

namespace SteamWishlistWidget.Tests;

public sealed class SettingsStoreTests : IDisposable
{
    private const string SteamId = "76561190000000001";
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "SteamWishlistWidget.Tests", Guid.NewGuid().ToString("N"));
    private readonly FakeSecretProtector _protector = new();
    private string ConfigPath => Path.Combine(_folder, "config.json");


    public void Dispose()
    {
        if (Directory.Exists(_folder))
            Directory.Delete(_folder, recursive: true);
    }

    [Fact]
    public void Save_EncryptsSteamId_AndLoadRestoresIt()
    {
        var settings = new AppSettings { SteamId = SteamId, Theme = ThemeNames.Light, VisibleRows = 8 };

        new SettingsStore(ConfigPath, _protector).Save(settings);
        var raw = File.ReadAllText(ConfigPath);
        var loaded = new SettingsStore(ConfigPath, _protector).Load();

        Assert.DoesNotContain(SteamId, raw);
        Assert.Contains("SteamIdProtected", raw);
        Assert.Equal(SteamId, loaded.SteamId);
        Assert.Equal(ThemeNames.Light, loaded.Theme);
        Assert.Equal(8, loaded.VisibleRows);
    }

    [Fact]
    public void Load_MigratesPlainTextSteamId_FromOldVersions()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(ConfigPath, $$"""{"SteamId":"{{SteamId}}","Theme":"Dark"}""");

        var loaded = new SettingsStore(ConfigPath, _protector).Load();
        var raw = File.ReadAllText(ConfigPath);

        Assert.Equal(SteamId, loaded.SteamId);
        Assert.DoesNotContain(SteamId, raw);
    }

    [Fact]
    public void Load_FixesInvalidValues()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(ConfigPath, """
            {"SteamId":"abc","Theme":"Neon","CountryCode":"Brasil","Language":"klingon",
             "VisibleRows":999,"RefreshMinutes":1}
            """);

        var loaded = new SettingsStore(ConfigPath, _protector).Load();

        Assert.Null(loaded.SteamId);
        Assert.Equal(ThemeNames.Glass, loaded.Theme);
        Assert.Equal(AppSettings.DefaultCountry, loaded.CountryCode);
        Assert.Equal(AppSettings.DefaultLanguage, loaded.Language);
        Assert.Equal(AppSettings.DefaultVisibleRows, loaded.VisibleRows);
        Assert.Equal(AppSettings.DefaultRefreshMinutes, loaded.RefreshMinutes);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("")]
    public void Load_ReturnsDefaults_ForCorruptedFile(string content)
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(ConfigPath, content);

        var loaded = new SettingsStore(ConfigPath, _protector).Load();

        Assert.Null(loaded.SteamId);
        Assert.Equal(ThemeNames.Glass, loaded.Theme);
    }
}
