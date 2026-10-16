using OkinawaBot.Application.Models;
using OkinawaBot.Application.Services;
using OkinawaBot.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OkinawaBot.Tests.Services;

public class QueryServiceTests
{
    //驗證查詢資料存在
    [Fact]
    public async Task QueryByCategoryAsync_ShouldReturnMatchingItems()
    {
        // Arrange
        var repository =
            new FakeTravelItemRepository();

        var saveService =
            new SaveService(repository);

        await saveService.SaveAsync(
            new SaveTravelItemRequest
            {
                Url = "https://example.com/churaumi",
                Category = "Attraction",
                Name = "美麗海水族館"
            });
    }

    //驗證查詢資料不存在
    [Fact]
    public async Task QueryByCategoryAsync_ShouldReturnEmpty_WhenNoItemsFound()
    {
        // Arrange
        var repository =
            new FakeTravelItemRepository();

        var service =
            new QueryService(repository);

        // Act
        var result =
            await service.QueryByCategoryAsync(
                "Restaurant");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task QueryByCategoryAsync_ShouldFilterByDestination_WhenDestinationIsSpecified()
    {
        // Arrange
        var repository = new FakeTravelItemRepository();
        var saveService = new SaveService(repository);
        var queryService = new QueryService(repository);

        await saveService.SaveAsync(new SaveTravelItemRequest
        {
            Destination = "沖繩",
            Category = "Attraction",
            Name = "美麗海水族館",
            Url = "https://example.com/okinawa"
        });

        await saveService.SaveAsync(new SaveTravelItemRequest
        {
            Destination = "東京",
            Category = "Attraction",
            Name = "東京鐵塔",
            Url = "https://example.com/tokyo"
        });

        // Act
        var okinawaItems = await queryService.QueryByCategoryAsync("Attraction", "沖繩");
        var tokyoItems = await queryService.QueryByCategoryAsync("Attraction", "東京");

        // Assert
        Assert.Single(okinawaItems);
        Assert.Equal("美麗海水族館", okinawaItems[0].Name);

        Assert.Single(tokyoItems);
        Assert.Equal("東京鐵塔", tokyoItems[0].Name);
    }

    [Fact]
    public async Task GetAvailableDestinationsAsync_ShouldReturnDistinctDestinations()
    {
        // Arrange
        var repository = new FakeTravelItemRepository();
        var saveService = new SaveService(repository);
        var queryService = new QueryService(repository);

        await saveService.SaveAsync(new SaveTravelItemRequest
        {
            Destination = "沖繩",
            Category = "Food",
            Name = "拉麵",
            Url = "https://example.com/1"
        });

        await saveService.SaveAsync(new SaveTravelItemRequest
        {
            Destination = "東京",
            Category = "Food",
            Name = "壽司",
            Url = "https://example.com/2"
        });

        // Act
        var destinations = await queryService.GetAvailableDestinationsAsync();

        // Assert
        Assert.Equal(2, destinations.Count);
        Assert.Contains("沖繩", destinations);
        Assert.Contains("東京", destinations);
    }
}
