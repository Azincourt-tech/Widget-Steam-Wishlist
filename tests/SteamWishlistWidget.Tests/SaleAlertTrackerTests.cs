using SteamWishlistWidget.Models;
using SteamWishlistWidget.Services;

namespace SteamWishlistWidget.Tests;

public class SaleAlertTrackerTests
{
    private static GameDeal Deal(int id, int discount) =>
        new(id, $"Jogo {id}", "R$10,00", discount > 0 ? "R$20,00" : null, discount, 1000, null);

    [Fact]
    public void FirstRead_IsOnlyTheBaseline()
    {
        var tracker = new SaleAlertTracker();

        Assert.Empty(tracker.Update([Deal(1, 50), Deal(2, 0)]));
    }

    [Fact]
    public void ReturnsOnlyGamesThatJustEnteredSale()
    {
        var tracker = new SaleAlertTracker();
        tracker.Update([Deal(1, 50), Deal(2, 0), Deal(3, 0)]);

        var entered = tracker.Update([Deal(1, 50), Deal(2, 30), Deal(3, 0)]);

        Assert.Equal([2], entered.Select(d => d.AppId));
    }

    [Fact]
    public void GameAddedToListAlreadyOnSale_Notifies()
    {
        var tracker = new SaleAlertTracker();
        tracker.Update([Deal(1, 0)]);

        var entered = tracker.Update([Deal(1, 0), Deal(4, 25)]);

        Assert.Equal([4], entered.Select(d => d.AppId));
    }

    [Fact]
    public void LeavingAndReturningToSale_NotifiesAgain()
    {
        var tracker = new SaleAlertTracker();
        tracker.Update([Deal(1, 50)]);
        tracker.Update([Deal(1, 0)]);

        var entered = tracker.Update([Deal(1, 40)]);

        Assert.Equal([1], entered.Select(d => d.AppId));
    }

    [Fact]
    public void Reset_MakesNextReadTheBaselineAgain()
    {
        var tracker = new SaleAlertTracker();
        tracker.Update([Deal(1, 0)]);

        tracker.Reset();

        Assert.Empty(tracker.Update([Deal(1, 50)]));
    }
}
