using SteamWishlistWidget.Services;

namespace SteamWishlistWidget.Tests;

public class SteamApiClientParsingTests
{
    [Fact]
    public void ParseWishlistAppIds_ReadsIds_AndIgnoresDuplicatesAndInvalid()
    {
        const string json = """
            {"response":{"items":[{"appid":10},{"appid":20},{"appid":10},{"appid":-1},{"priority":1}]}}
            """;

        Assert.Equal([10, 20], SteamApiClient.ParseWishlistAppIds(json));
    }

    [Theory]
    [InlineData("""{"response":{}}""")]
    [InlineData("""{"response":{"items":"x"}}""")]
    [InlineData("""{}""")]
    public void ParseWishlistAppIds_ReturnsEmpty_ForPrivateOrUnexpectedResponse(string json)
    {
        Assert.Empty(SteamApiClient.ParseWishlistAppIds(json));
    }

    [Fact]
    public void ParseStoreItems_ReadsPriceDiscountAndImage()
    {
        const string json = """
            {"response":{"store_items":[
              {"appid":1091500,"name":"Cyberpunk 2077",
               "assets":{"asset_url_format":"steam/apps/1091500/${FILENAME}?t=1","small_capsule":"capsule_231x87.jpg"},
               "best_purchase_option":{"final_price_in_cents":"5997","formatted_final_price":"R$59,97",
                                       "formatted_original_price":"R$199,90","discount_pct":70}},
              {"appid":2,"name":"Sem preço"}
            ]}}
            """;

        var deal = Assert.Single(SteamApiClient.ParseStoreItems(json));

        Assert.Equal(1091500, deal.AppId);
        Assert.Equal("R$59,97", deal.FinalPrice);
        Assert.Equal("R$199,90", deal.OriginalPrice);
        Assert.Equal(70, deal.DiscountPct);
        Assert.Equal(5997, deal.FinalCents);
        Assert.Equal(
            "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1091500/capsule_231x87.jpg?t=1",
            deal.ImageUrl);
    }

    [Fact]
    public void ParseStoreItems_ClampsDiscount_AndKeepsImageOnSteamServers()
    {
        // Um formato malicioso não consegue trocar o servidor da imagem.
        const string json = """
            {"response":{"store_items":[
              {"appid":5,"name":"X",
               "assets":{"asset_url_format":"@evil.com/../../${FILENAME}","small_capsule":"a.jpg"},
               "best_purchase_option":{"final_price_in_cents":"100","formatted_final_price":"R$1,00","discount_pct":250}}
            ]}}
            """;

        var deal = Assert.Single(SteamApiClient.ParseStoreItems(json));

        Assert.Equal(100, deal.DiscountPct);
        Assert.Equal("shared.akamai.steamstatic.com", new Uri(deal.ImageUrl!).Host);
    }

    [Fact]
    public void ParseSteamIdFromProfileXml_ReadsId()
    {
        const string xml = "<?xml version=\"1.0\"?><profile><steamID64>76561190000000002</steamID64></profile>";

        Assert.Equal("76561190000000002", SteamApiClient.ParseSteamIdFromProfileXml(xml));
    }

    [Fact]
    public void ParseSteamIdFromProfileXml_RejectsDtd_ToPreventXxe()
    {
        const string xml = """
            <?xml version="1.0"?>
            <!DOCTYPE profile [<!ENTITY xxe SYSTEM "file:///c:/windows/win.ini">]>
            <profile><steamID64>&xxe;</steamID64></profile>
            """;

        Assert.Null(SteamApiClient.ParseSteamIdFromProfileXml(xml));
    }

    [Fact]
    public void ParseSteamIdFromProfileXml_ReturnsNull_ForErrorPage()
    {
        Assert.Null(SteamApiClient.ParseSteamIdFromProfileXml("<response><error>The specified profile could not be found.</error></response>"));
        Assert.Null(SteamApiClient.ParseSteamIdFromProfileXml("<html>not xml"));
    }
}
