using SteamWishlistWidget.Services;

namespace SteamWishlistWidget.Tests;

public class SteamIdParserTests
{
    private const string ValidId = "76561190000000001";

    [Theory]
    [InlineData(ValidId)]
    [InlineData("  " + ValidId + "  ")]
    [InlineData("https://steamcommunity.com/profiles/" + ValidId)]
    [InlineData("https://steamcommunity.com/profiles/" + ValidId + "/")]
    [InlineData("steamcommunity.com/profiles/" + ValidId + "/wishlist")]
    public void Parse_ReturnsSteamId_ForIdOrProfileLink(string input)
    {
        var result = SteamIdParser.Parse(input);

        Assert.Equal(ValidId, result.SteamId);
        Assert.Null(result.VanityName);
    }

    [Theory]
    [InlineData("https://steamcommunity.com/id/perfil-exemplo", "perfil-exemplo")]
    [InlineData("https://steamcommunity.com/id/perfil-exemplo/", "perfil-exemplo")]
    [InlineData("perfil-exemplo", "perfil-exemplo")]
    [InlineData("meu_nome-123", "meu_nome-123")]
    public void Parse_ReturnsVanityName_ForCustomUrl(string input, string expected)
    {
        var result = SteamIdParser.Parse(input);

        Assert.Null(result.SteamId);
        Assert.Equal(expected, result.VanityName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("https://example.com/id/alguem")]
    [InlineData("nome com espaço")]
    [InlineData("../../etc/passwd")]
    [InlineData("<script>")]
    public void Parse_IsInvalid_ForUnexpectedInput(string? input)
    {
        Assert.False(SteamIdParser.Parse(input).IsValid);
    }

    [Theory]
    [InlineData(ValidId, true)]
    [InlineData("12345678901234567", false)]
    [InlineData("7656119000000001", false)]
    [InlineData("765611900000000010", false)]
    [InlineData(null, false)]
    public void IsValidSteamId64_ChecksFormat(string? value, bool expected)
    {
        Assert.Equal(expected, SteamIdParser.IsValidSteamId64(value));
    }
}
