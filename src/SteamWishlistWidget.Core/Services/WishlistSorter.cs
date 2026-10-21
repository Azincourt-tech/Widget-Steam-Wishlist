using SteamWishlistWidget.Models;

namespace SteamWishlistWidget.Services;

public static class WishlistSorter
{
    /// <summary>
    /// Promoções primeiro (maior desconto antes), depois do mais barato ao mais caro.
    /// </summary>
    public static List<GameDeal> Sort(IEnumerable<GameDeal> deals, bool onlyDiscounted)
    {
        if (onlyDiscounted)
            deals = deals.Where(d => d.IsDiscounted);

        return deals
            .OrderByDescending(d => d.IsDiscounted)
            .ThenByDescending(d => d.DiscountPct)
            .ThenBy(d => d.FinalCents)
            .ThenBy(d => d.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
