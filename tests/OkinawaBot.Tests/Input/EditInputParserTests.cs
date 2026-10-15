using OkinawaBot.Application.Input;

namespace OkinawaBot.Tests.Input;

public class EditInputParserTests
{
    private readonly EditInputParser _parser = new();

    [Fact]
    public void Parse_ShouldReturnFailure_WhenInputIsEmptyOrWhitespace()
    {
        var result = _parser.Parse("   ");

        Assert.False(result.IsSuccess);
        Assert.Contains("不可以是空的", result.ErrorMessage);
    }

    [Fact]
    public void Parse_ShouldReturnFailure_WhenInputIsOnlyHash()
    {
        var result = _parser.Parse("#");

        Assert.False(result.IsSuccess);
        Assert.Contains("缺少分類名稱", result.ErrorMessage);
    }

    [Fact]
    public void Parse_ShouldParseAllThreeFields_SingleLine()
    {
        var result = _parser.Parse("https://example.com/aquarium #Attraction 美麗海水族館");

        Assert.True(result.IsSuccess);
        Assert.Equal("https://example.com/aquarium", result.Url);
        Assert.Equal("Attraction", result.Category);
        Assert.Equal("美麗海水族館", result.Name);
    }

    [Fact]
    public void Parse_ShouldParseAllThreeFields_MultiLine()
    {
        var input = """
            https://example.com/aquarium
            #Attraction
            美麗海水族館
            """;

        var result = _parser.Parse(input);

        Assert.True(result.IsSuccess);
        Assert.Equal("https://example.com/aquarium", result.Url);
        Assert.Equal("Attraction", result.Category);
        Assert.Equal("美麗海水族館", result.Name);
    }

    [Fact]
    public void Parse_ShouldParseOnlyUrl()
    {
        var result = _parser.Parse("https://example.com/new-url");

        Assert.True(result.IsSuccess);
        Assert.Equal("https://example.com/new-url", result.Url);
        Assert.Null(result.Category);
        Assert.Null(result.Name);
    }

    [Fact]
    public void Parse_ShouldParseOnlyCategory()
    {
        var result = _parser.Parse("#Food");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Url);
        Assert.Equal("Food", result.Category);
        Assert.Null(result.Name);
    }

    [Fact]
    public void Parse_ShouldParseOnlyName()
    {
        var result = _parser.Parse("古宇利大橋");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Url);
        Assert.Null(result.Category);
        Assert.Equal("古宇利大橋", result.Name);
    }

    [Fact]
    public void Parse_ShouldParseUrlAndCategory()
    {
        var result = _parser.Parse("https://example.com/food #Food");

        Assert.True(result.IsSuccess);
        Assert.Equal("https://example.com/food", result.Url);
        Assert.Equal("Food", result.Category);
        Assert.Null(result.Name);
    }

    [Fact]
    public void Parse_ShouldParseCategoryAndName()
    {
        var result = _parser.Parse("#Food 暖暮拉麵");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Url);
        Assert.Equal("Food", result.Category);
        Assert.Equal("暖暮拉麵", result.Name);
    }

    [Fact]
    public void Parse_ShouldParseUrlAndName()
    {
        var result = _parser.Parse("https://example.com/ramen 暖暮拉麵");

        Assert.True(result.IsSuccess);
        Assert.Equal("https://example.com/ramen", result.Url);
        Assert.Null(result.Category);
        Assert.Equal("暖暮拉麵", result.Name);
    }

    [Fact]
    public void Parse_ShouldSupportExplicitPrefixes()
    {
        var input = """
            名稱: 新店名
            類別: 美食
            網址: https://example.com/prefix
            """;

        var result = _parser.Parse(input);

        Assert.True(result.IsSuccess);
        Assert.Equal("https://example.com/prefix", result.Url);
        Assert.Equal("美食", result.Category);
        Assert.Equal("新店名", result.Name);
    }

    [Fact]
    public void Parse_ShouldFail_WhenUrlIsInvalidScheme()
    {
        var result = _parser.Parse("ftp://invalid.com #Food 店名");

        Assert.False(result.IsSuccess);
        Assert.Contains("URL 格式不正確", result.ErrorMessage);
    }
}
