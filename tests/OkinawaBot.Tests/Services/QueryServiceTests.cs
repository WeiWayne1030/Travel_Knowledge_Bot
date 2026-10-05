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
}
