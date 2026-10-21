using System.Net.Http;
using SteamWishlistWidget.Models;
using SteamWishlistWidget.Tests.Fakes;
using SteamWishlistWidget.ViewModels;

namespace SteamWishlistWidget.Tests.ViewModels;

public class WidgetViewModelTests
{
    private const string SteamId = "76561190000000001";

    private readonly FakeSteamApiClient _steam = new();
    private readonly FakeSettingsStore _store = new();
    private readonly FakeStoreLauncher _launcher = new();
    private readonly FakeThemeService _themes = new();
    private readonly FakeDialogService _dialogs = new();
    private readonly FakeUiTimer _timer = new();

    private WidgetViewModel Create(AppSettings settings) =>
        new(settings, _steam, _store, _launcher, _themes, _dialogs, _timer);

    private static GameDeal Deal(int id, int discount) =>
        new(id, $"Jogo {id}", "R$10,00", discount > 0 ? "R$20,00" : null, discount, 1000, null);

    [Fact]
    public async Task Initialize_WithoutProfile_ShowsSetup()
    {
        var vm = Create(new AppSettings { Collapsed = true });

        await vm.InitializeAsync();

        Assert.True(vm.IsSetupVisible);
        Assert.False(vm.IsCollapsed);
        Assert.Equal(0, _steam.WishlistCalls);
    }

    [Fact]
    public async Task Initialize_WithProfile_LoadsDealsAndCountsSales()
    {
        _steam.Deals = [Deal(1, 50), Deal(2, 0), Deal(3, 10)];
        var vm = Create(new AppSettings { SteamId = SteamId });

        await vm.InitializeAsync();
        vm.Shutdown();

        Assert.Equal(3, vm.Deals.Count);
        Assert.Equal(2, vm.OnSaleCount);
        Assert.True(vm.HasSaleBadge);
        Assert.StartsWith("2 de 3 em promoção", vm.Subtitle);
        Assert.False(vm.HasStatus);
    }

    [Fact]
    public async Task Refresh_NetworkError_ShowsFriendlyMessage()
    {
        _steam.WishlistError = new HttpRequestException("falhou");
        var vm = Create(new AppSettings { SteamId = SteamId });

        await vm.RefreshCommand.ExecuteAsync(null);

        Assert.True(vm.HasStatus);
        Assert.Contains("sem conexão", vm.StatusMessage);
    }

    [Fact]
    public async Task Connect_ResolvesProfile_SavesAndLoads()
    {
        _steam.Resolve = _ => SteamId;
        _steam.Deals = [Deal(1, 0)];
        var vm = Create(new AppSettings());
        await vm.InitializeAsync();

        Assert.False(vm.ConnectCommand.CanExecute(null));
        vm.ProfileInput = "https://steamcommunity.com/id/alguem";
        await vm.ConnectCommand.ExecuteAsync(null);
        vm.Shutdown();

        Assert.False(vm.IsSetupVisible);
        Assert.Equal(SteamId, _store.LastSaved?.SteamId);
        Assert.Single(vm.Deals);
    }

    [Fact]
    public void TogglePin_And_Collapse_AreSaved()
    {
        var vm = Create(new AppSettings { SteamId = SteamId });

        vm.TogglePinCommand.Execute(null);
        vm.ToggleCollapsedCommand.Execute(null);

        Assert.True(vm.IsPinned);
        Assert.Equal("Desafixar", vm.PinToolTip);
        Assert.True(vm.IsCollapsed);
        Assert.Equal("Abrir lista", vm.CollapseMenuHeader);
        Assert.True(_store.LastSaved?.Pinned);
        Assert.True(_store.LastSaved?.Collapsed);
    }

    [Fact]
    public void OpenDeal_OpensStorePage()
    {
        var vm = Create(new AppSettings());

        vm.OpenDealCommand.Execute(Deal(570, 0));
        vm.OpenDealCommand.Execute(null);

        Assert.Equal([570], _launcher.Opened);
    }

    [Fact]
    public async Task OpenSettings_Saved_AppliesThemeRowsAndReloads()
    {
        _steam.Deals = [Deal(1, 0)];
        var vm = Create(new AppSettings { SteamId = SteamId, Language = "brazilian" });
        _dialogs.OnShowSettings = s =>
        {
            s.Theme = ThemeNames.Light;
            s.VisibleRows = 10;
            s.Language = "english";
            return true;
        };

        await vm.OpenSettingsCommand.ExecuteAsync(null);
        vm.Shutdown();

        Assert.Equal(ThemeNames.Light, vm.Theme);
        Assert.Contains(ThemeNames.Light, _themes.Applied);
        Assert.Equal(10 * WidgetViewModel.RowHeight, vm.ListMaxHeight);
        Assert.Equal(1, _steam.WishlistCalls);
    }

    [Fact]
    public async Task OpenSettings_Cancelled_RestoresSavedTheme()
    {
        var vm = Create(new AppSettings { SteamId = SteamId, Theme = ThemeNames.Dark });
        _dialogs.OnShowSettings = _ => false;

        await vm.OpenSettingsCommand.ExecuteAsync(null);

        Assert.Equal(ThemeNames.Dark, _themes.Applied.Last());
        Assert.Equal(0, _steam.WishlistCalls);
    }

    [Fact]
    public void Close_RaisesCloseRequested()
    {
        var vm = Create(new AppSettings());
        var closed = false;
        vm.CloseRequested += (_, _) => closed = true;

        vm.CloseCommand.Execute(null);

        Assert.True(closed);
    }
}
