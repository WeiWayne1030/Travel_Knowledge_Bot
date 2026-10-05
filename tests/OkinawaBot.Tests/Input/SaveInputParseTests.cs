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
        Assert.Equal(
            "缺少分類，請使用 #分類 格式，例如 #Attraction。",
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
        Assert.Equal(
            "缺少 URL，請將 URL 放在 #分類 之前。",
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
        Assert.Equal(
            "缺少名稱，請將名稱放在 #分類 之後。",
            result.ErrorMessage);
    }
}