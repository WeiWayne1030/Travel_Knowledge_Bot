using OkinawaBot.Application.Input;
using OkinawaBot.Application.Models;

namespace OkinawaBot.Tests.Input;

public class SaveInputParserTests
{
    //正常輸入
    [Fact]
    public void Parse_ShouldReturnSuccess_WhenInputIsValid()
    {
        // Arrange
        var parser = new SaveInputParser();

        var input =
            "https://example.com #ATTRACTION 美麗海水族館";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(
            "https://example.com",
            result.Url);

        Assert.Equal(
            "ATTRACTION",
            result.Category);

        Assert.Equal(
            "美麗海水族館",
            result.Name);
    }

    //沒有Category
    [Fact]
    public void Parse_ShouldReturnFailure_WhenCategoryIsMissing()
    {
        // Arrange
        var parser = new SaveInputParser();

        var input =
            "https://example.com 美麗海水族館";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(
            "缺少分類，請使用 #分類 格式，例如 #Attraction。",
            result.ErrorMessage);
        Assert.Contains(
            "正確格式範例：",
            result.ErrorMessage);
    }

    //沒有url
    [Fact]
    public void Parse_ShouldReturnFailure_WhenUrlIsMissing()
    {
        // Arrange
        var parser = new SaveInputParser();

        var input =
            "#ATTRACTION 美麗海水族館";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(
            "缺少 URL，請將 URL 放在 #分類 之前。",
            result.ErrorMessage);
        Assert.Contains(
            "正確格式範例：",
            result.ErrorMessage);
    }

    //沒有name
    [Fact]
    public void Parse_ShouldReturnFailure_WhenNameIsMissing()
    {
        // Arrange
        var parser = new SaveInputParser();

        var input =
            "https://example.com #ATTRACTION";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(
            "缺少名稱，請將名稱放在 #分類 之後。",
            result.ErrorMessage);
        Assert.Contains(
            "正確格式範例：",
            result.ErrorMessage);
    }

    // URL 格式無效 (非 http/https 或隨意字串)
    [Theory]
    [InlineData("not-a-valid-url #ATTRACTION 美麗海水族館")]
    [InlineData("ftp://example.com/file #ATTRACTION 美麗海水族館")]
    public void Parse_ShouldReturnFailure_WhenUrlIsInvalid(string input)
    {
        // Arrange
        var parser = new SaveInputParser();

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(
            "URL 格式不正確，必須以 http:// 或 https:// 開頭。",
            result.ErrorMessage);
        Assert.Contains(
            "正確格式範例：",
            result.ErrorMessage);
    }
}