namespace SteamWishlistWidget.Models;

/// <summary>
/// Um jogo da lista de desejos com o preço atual na loja.
/// </summary>
public record GameDeal(
    int AppId,
    string Name,
    string FinalPrice,
    string? OriginalPrice,
    int DiscountPct,
    long FinalCents,
    string? ImageUrl)
{
    public bool IsDiscounted => DiscountPct > 0;
    public string DiscountText => $"-{DiscountPct}%";
}
