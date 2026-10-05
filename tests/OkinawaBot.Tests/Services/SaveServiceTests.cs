using OkinawaBot.Application.Models;
using OkinawaBot.Application.Services;
using OkinawaBot.Tests.Fakes;

namespace OkinawaBot.Tests.Services;

public class SaveServiceTests
{
    //測名稱
    [Fact]
    public async Task SaveAsync_ShouldReturnValidationError_WhenNameIsEmpty()
    {
        // Arrange
        var repository = new FakeTravelItemRepository();
        var service = new SaveService(repository);

        var request = new SaveTravelItemRequest
        {
            Name = "",
            Url = "https://example.com",
            Category = "ATTRACTION"
        };

        // Act
        var result = await service.SaveAsync(request);

        // Assert
        Assert.Equal(
            SaveResultStatus.ValidationError,
            result.Status);
    }

    //測URL
    [Fact]
    public async Task SaveAsync_ShouldReturnValidationError_WhenUrlIsEmpty()
    {
        // Arrange
        var repository = new FakeTravelItemRepository();
        var service = new SaveService(repository);

        var request = new SaveTravelItemRequest
        {
            Name = "Churaumi Aquarium",
            Url = "",
            Category = "ATTRACTION"
        };

        // Act
        var result = await service.SaveAsync(request);

        // Assert
        Assert.Equal(
            SaveResultStatus.ValidationError,
            result.Status);
    }

    //測分類
    [Fact]
    public async Task SaveAsync_ShouldReturnValidationError_WhenCategoryIsEmpty()
    {
        // Arrange
        var repository = new FakeTravelItemRepository();
        var service = new SaveService(repository);

        var request = new SaveTravelItemRequest
        {
            Name = "Churaumi Aquarium",
            Url = "https://example.com",
            Category = ""
        };

        // Act
        var result = await service.SaveAsync(request);

        // Assert
        Assert.Equal(
            SaveResultStatus.ValidationError,
            result.Status);
    }

    //測名稱是否存在
    [Fact]
    public async Task SaveAsync_ShouldReturnDuplicate_WhenNameAlreadyExists()
    {
        // Arrange
        var repository = new FakeTravelItemRepository();

        await repository.CreateAsync(new OkinawaBot.Domain.Entities.TravelItem
        {
            Name = "Churaumi Aquarium",
            Url = "https://example.com/old",
            Category = "ATTRACTION"
        });

        var service = new SaveService(repository);

        var request = new SaveTravelItemRequest
        {
            Name = "Churaumi Aquarium",
            Url = "https://example.com/new",
            Category = "ATTRACTION"
        };

        // Act
        var result = await service.SaveAsync(request);

        // Assert
        Assert.Equal(
            SaveResultStatus.Duplicate,
            result.Status);
    }

    //正常情況
    [Fact]
    public async Task SaveAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var repository = new FakeTravelItemRepository();

        var service = new SaveService(repository);

        var request = new SaveTravelItemRequest
        {
            Name = "美麗海水族館",
            Url = "https://example.com",
            Category = "Attraction"
        };

        // Act
        var result = await service.SaveAsync(request);

        // Assert
        Assert.Equal(
            SaveResultStatus.Success,
            result.Status);

        Assert.NotNull(result.ItemId);
    }

    //測試確認除儲存相同的項目
    [Fact]
    public async Task SaveAsync_ShouldCreateItem_WhenDuplicateCheckIsSkipped()
    {
        // Arrange
        var repository =
            new FakeTravelItemRepository();

        var service =
            new SaveService(repository);

        var request =
            new SaveTravelItemRequest
            {
                Url = "https://example.com/okinawa",
                Category = "Attraction",
                Name = "美麗海水族館"
            };

        // Act
        var result =
            await service.SaveAsync(
                request,
                skipDuplicateCheck: true);

        // Assert
        Assert.Equal(
            SaveResultStatus.Success,
            result.Status);

        Assert.Single(repository.Items);

        Assert.Equal(
            "美麗海水族館",
            repository.Items[0].Name);

        Assert.Equal(
            "Attraction",
            repository.Items[0].Category);

        Assert.Equal(
            "https://example.com/okinawa",
            repository.Items[0].Url);
    }
}