using OkinawaBot.Application.Services;
using OkinawaBot.Domain.Entities;
using OkinawaBot.Tests.Fakes;

namespace OkinawaBot.Tests.Services;

public class DeleteServiceTests
{
    //建立刪除
    [Fact]
    public async Task DeleteAsync_WhenItemExists_ShouldDeleteItem()
    {
        // Arrange
        var repository = new FakeTravelItemRepository();

        var item = new TravelItem
        {
            Id = 1,
            Name = "美麗海水族館",
            Url = "https://example.com",
            Category = "Attraction"
        };

        await repository.CreateAsync(item);

        var service = new DeleteService(repository);

        // Act
        var result = await service.DeleteAsync(1);

        // Assert
        Assert.True(result);

        var deletedItem =
            await repository.FindByIdAsync(1);

        Assert.Null(deletedItem);
    }

    //要刪除的資料不存在
    [Fact]
    public async Task DeleteAsync_WhenItemDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var repository = new FakeTravelItemRepository();

        var service = new DeleteService(repository);

        // Act
        var result = await service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetDeletableItemsAsync_ShouldReturnAllItems()
    {
        // Arrange
        var repository = new FakeTravelItemRepository();

        await repository.CreateAsync(new TravelItem
        {
            Id = 1,
            Name = "美麗海水族館",
            Category = "Attraction",
            Url = "https://example.com/1"
        });

        await repository.CreateAsync(new TravelItem
        {
            Id = 2,
            Name = "國際通",
            Category = "Attraction",
            Url = "https://example.com/2"
        });

        var service = new DeleteService(repository);

        // Act
        var result =
            await service.GetDeletableItemsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("美麗海水族館", result[0].Name);
        Assert.Equal("國際通", result[1].Name);
    }
}