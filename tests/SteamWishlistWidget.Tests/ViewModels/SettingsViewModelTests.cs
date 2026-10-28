using SteamWishlistWidget.Models;
using SteamWishlistWidget.Tests.Fakes;
using SteamWishlistWidget.ViewModels;

namespace SteamWishlistWidget.Tests.ViewModels;

public class SettingsViewModelTests
{
    private const string SteamId = "76561190000000001";
    private const string OtherSteamId = "76561190000000002";

    private readonly FakeSteamApiClient _steam = new();
    private readonly FakeSettingsStore _store = new();
    private readonly FakeStartupRegistration _startup = new();
    private readonly FakeThemeService _themes = new();

    private SettingsViewModel Create(AppSettings settings) => new(settings, _steam, _store, _startup, _themes);

    [Fact]
    public void LoadsCurrentValues_WithoutApplyingTheme()
    {
        var vm = Create(new AppSettings { SteamId = SteamId, Theme = ThemeNames.Light, VisibleRows = 8 });

        Assert.Equal(ThemeNames.Light, vm.Theme);
        Assert.EndsWith(SteamId, vm.ProfileInput);
        Assert.Equal(8, vm.VisibleRows);
        Assert.Empty(_themes.Applied);
    }

    [Fact]
    public void ChangingTheme_PreviewsImmediately()
    {
        var vm = Create(new AppSettings());

        vm.Theme = ThemeNames.Dark;

        Assert.Equal([ThemeNames.Dark], _themes.Applied);
    }

    [Fact]
    public async Task Save_InvalidCountry_ShowsErrorAndKeepsSettings()
    {
        var settings = new AppSettings();
        var vm = Create(settings);
        bool? result = null;
        vm.CloseRequested += (_, saved) => result = saved;

        vm.CountryCode = "Brasil";
        await vm.SaveCommand.ExecuteAsync(null);

        Assert.True(vm.HasError);
        Assert.Null(result);
        Assert.Null(_store.LastSaved);
        Assert.Equal(AppSettings.DefaultCountry, settings.CountryCode);
    }

    [Fact]
    public async Task Save_NewProfile_ResolvesAndSavesEverything()
    {
        _steam.Resolve = _ => OtherSteamId;
        var settings = new AppSettings { SteamId = SteamId };
        var vm = Create(settings);
        bool? result = null;
        vm.CloseRequested += (_, saved) => result = saved;

        vm.ProfileInput = "https://steamcommunity.com/id/outro";
        vm.CountryCode = "us";
        vm.RefreshMinutes = 30;
        vm.StartWithWindows = true;
        vm.Notifications = false;
        await vm.SaveCommand.ExecuteAsync(null);

        Assert.True(result);
        Assert.Equal(OtherSteamId, settings.SteamId);
        Assert.Equal("US", settings.CountryCode);
        Assert.Equal(30, settings.RefreshMinutes);
        Assert.False(settings.Notifications);
        Assert.False(_store.LastSaved?.Notifications);
        Assert.Equal(OtherSteamId, _store.LastSaved?.SteamId);
        Assert.True(_startup.IsEnabled);
    }

    [Fact]
    public async Task Save_ProfileNotFound_ShowsErrorAndKeepsOldProfile()
    {
        _steam.Resolve = _ => throw new InvalidOperationException("Perfil não encontrado.");
        var settings = new AppSettings { SteamId = SteamId };
        var vm = Create(settings);

        vm.ProfileInput = "perfil-que-nao-existe";
        await vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Perfil não encontrado.", vm.ErrorMessage);
        Assert.Equal(SteamId, settings.SteamId);
    }

    [Fact]
    public void Cancel_ClosesWithoutSaving()
    {
        var vm = Create(new AppSettings());
        bool? result = null;
        vm.CloseRequested += (_, saved) => result = saved;

        vm.CancelCommand.Execute(null);

        Assert.False(result);
        Assert.Null(_store.LastSaved);
    }
}
