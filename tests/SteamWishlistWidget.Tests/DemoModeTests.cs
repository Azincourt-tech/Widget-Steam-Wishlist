using SteamWishlistWidget.Models;
using SteamWishlistWidget.Services;
using SteamWishlistWidget.Services.Demo;

namespace SteamWishlistWidget.Tests;

public class DemoModeTests
{
    [Theory]
    [InlineData(new string[0], false, null)]
    [InlineData(new[] { "--demo" }, true, null)]
    [InlineData(new[] { "--DEMO", "--theme=Light" }, true, ThemeNames.Light)]
    [InlineData(new[] { "--demo", "--theme=qualquer" }, true, ThemeNames.Glass)]
    public void StartupOptions_ParsesArguments(string[] args, bool demo, string? theme)
    {
        var options = StartupOptions.Parse(args);

        Assert.Equal(demo, options.Demo);
        Assert.Equal(theme, options.Theme);
    }

    [Fact]
    public async Task DemoClient_ReturnsSortedFictitiousGames()
    {
        var client = new DemoSteamApiClient();

        var deals = await client.GetWishlistDealsAsync(new AppSettings());

        Assert.Equal(10, deals.Count);
        Assert.Equal(75, deals[0].DiscountPct);
        Assert.Equal("R$299,90", deals[0].OriginalPrice);
        Assert.Equal(deals, WishlistSorter.Sort(deals, onlyDiscounted: false));
    }

    [Fact]
    public async Task DemoClient_UsesFictitiousSteamId()
    {
        var id = await new DemoSteamApiClient().ResolveSteamIdAsync("qualquer coisa");

        Assert.Equal(DemoSteamApiClient.DemoSteamId, id);
        Assert.True(SteamIdParser.IsValidSteamId64(id));
    }

    [Fact]
    public void InMemoryStore_DoesNotShareInstances()
    {
        var store = new InMemorySettingsStore(new AppSettings { VisibleRows = 3 });

        var loaded = store.Load();
        loaded.VisibleRows = 15;

        Assert.Equal(3, store.Load().VisibleRows);
    }
}
