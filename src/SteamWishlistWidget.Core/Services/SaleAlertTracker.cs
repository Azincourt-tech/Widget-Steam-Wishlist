using SteamWishlistWidget.Models;

namespace SteamWishlistWidget.Services;

/// <summary>
/// Descobre quais jogos <b>entraram agora</b> em promoção entre uma leitura da lista e a seguinte.
/// A primeira leitura (e a primeira depois de <see cref="Reset"/>) é só a base: não gera avisos.
/// </summary>
public sealed class SaleAlertTracker
{
    private HashSet<int>? _onSale;

    /// <summary>Esquece a base (troca de perfil, país ou idioma): a próxima leitura volta a ser só base.</summary>
    public void Reset() => _onSale = null;

    public IReadOnlyList<GameDeal> Update(IReadOnlyList<GameDeal> deals)
    {
        var current = deals.Where(d => d.IsDiscounted).Select(d => d.AppId).ToHashSet();
        var previous = _onSale;
        _onSale = current;

        if (previous is null)
            return [];

        return deals.Where(d => d.IsDiscounted && !previous.Contains(d.AppId)).ToList();
    }
}
