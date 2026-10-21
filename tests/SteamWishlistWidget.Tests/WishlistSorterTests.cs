using SteamWishlistWidget.Models;
using SteamWishlistWidget.Services;

namespace SteamWishlistWidget.Tests;

public class WishlistSorterTests
{
    private static GameDeal Deal(string name, int discount, long cents) =>
        new(name.GetHashCode(), name, $"R${cents / 100.0:0.00}", null, discount, cents, null);

    [Fact]
    public void Sort_PutsBiggestDiscountFirst_ThenCheapest()
    {
        var deals = new[]
        {
            Deal("Caro", 0, 20000),
            Deal("Promo 20", 20, 8000),
            Deal("Barato", 0, 1000),
            Deal("Promo 80", 80, 4000),
        };

        var sorted = WishlistSorter.Sort(deals, onlyDiscounted: false).Select(d => d.Name);

        Assert.Equal(["Promo 80", "Promo 20", "Barato", "Caro"], sorted);
    }

    [Fact]
    public void Sort_OnlyDiscounted_RemovesFullPriceGames()
    {
        var deals = new[] { Deal("A", 0, 100), Deal("B", 10, 100) };

        var sorted = WishlistSorter.Sort(deals, onlyDiscounted: true);

        Assert.Equal("B", Assert.Single(sorted).Name);
    }
}
