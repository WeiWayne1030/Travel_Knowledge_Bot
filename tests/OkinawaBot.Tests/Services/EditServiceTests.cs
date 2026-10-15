using OkinawaBot.Application.Models;
using OkinawaBot.Application.Services;
using OkinawaBot.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OkinawaBot.Tests.Services;

public class EditServiceTests
{
    //測試如果Item不存在
    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var repository =
            new FakeTravelItemRepository();

        var service =
            new EditService(repository);

        var request =
            new UpdateTravelItemRequest
            {
                Url = "https://example.com",
                Category = "Attraction",
                Name = "美麗海水族館"
            };

        // Act
        var result =
            await service.UpdateAsync(
                999,
                request);

        // Assert
        Assert.Equal(
            EditResultStatus.NotFound,
            result.Status);
    }

    //測試正常update(同樣的東西(ID)不變,而改動其他項目)
    [Fact]
    public async Task UpdateAsync_ShouldUpdateItem_WhenRequestIsValid()
    {
        // Arrange
        var repository =
            new FakeTravelItemRepository();

        var saveService =
            new SaveService(repository);

        var itemResult =
            await saveService.SaveAsync(
                new SaveTravelItemRequest
                {
                    Url = "https://example.com/old",
                    Category = "Attraction",
                    Name = "舊名稱"
                });

        var item =
            repository.Items.Single();

        var service =
            new EditService(repository);

        var request =
            new UpdateTravelItemRequest
            {
                Url = "https://example.com/new",
                Category = "Food",
                Name = "新名稱"
            };

        // Act
        var result =
            await service.UpdateAsync(
                item.Id,
                request);

        // Assert
        Assert.Equal(
            EditResultStatus.Success,
            result.Status);

        Assert.Single(repository.Items);

        var updatedItem =
            repository.Items.Single();

        Assert.Equal(
            "新名稱",
            updatedItem.Name);

        Assert.Equal(
            "Food",
            updatedItem.Category);

        Assert.Equal(
            "https://example.com/new",
            updatedItem.Url);
    }

    //測試資料重複(existingItem.Id != itemId)
    [Fact]
    public async Task UpdateAsync_ShouldReturnDuplicate_WhenNewNameBelongsToAnotherItem()
    {
        // Arrange
        var repository =
            new FakeTravelItemRepository();

        var saveService =
            new SaveService(repository);

        await saveService.SaveAsync(
            new SaveTravelItemRequest
            {
                Url = "https://example.com/1",
                Category = "Attraction",
                Name = "美麗海水族館"
            });

        await saveService.SaveAsync(
            new SaveTravelItemRequest
            {
                Url = "https://example.com/2",
                Category = "Attraction",
                Name = "古宇利島"
            });

        var itemToEdit =
            repository.Items
                .Single(x => x.Name == "美麗海水族館");

        var service =
            new EditService(repository);

        var request =
            new UpdateTravelItemRequest
            {
                Url = "https://example.com/1-new",
                Category = "Attraction",
                Name = "古宇利島"
            };

        // Act
        var result =
            await service.UpdateAsync(
                itemToEdit.Id,
                request);

        // Assert
        Assert.Equal(
            EditResultStatus.Duplicate,
            result.Status);

        Assert.Equal(
            "美麗海水族館",
            itemToEdit.Name);
    }

    //比照save, 詢問使用者是否要儲存相同項目, 確認後直接存取
    [Fact]
    public async Task UpdateAsync_ShouldUpdate_WhenDuplicateCheckIsSkipped()
    {
        // Arrange
        var repository =
            new FakeTravelItemRepository();

        var saveService =
            new SaveService(repository);

        await saveService.SaveAsync(
            new SaveTravelItemRequest
            {
                Url = "https://example.com/1",
                Category = "Attraction",
                Name = "美麗海水族館"
            });

        await saveService.SaveAsync(
            new SaveTravelItemRequest
            {
                Url = "https://example.com/2",
                Category = "Attraction",
                Name = "古宇利島"
            });

        var item =
            repository.Items
                .Single(x => x.Name == "美麗海水族館");

        var service =
            new EditService(repository);

        var request =
            new UpdateTravelItemRequest
            {
                Url = "https://example.com/1-new",
                Category = "Attraction",
                Name = "古宇里島"
            };

        // Act
        var result =
            await service.UpdateAsync(
                item.Id,
                request,
                skipDuplicateCheck: true);

        // Assert
        Assert.Equal(
            EditResultStatus.Success,
            result.Status);

        Assert.Equal(
            "古宇里島",
            item.Name);
    }

    // 測試僅更新部分欄位，其他欄位維持原樣
    [Fact]
    public async Task UpdateAsync_ShouldOnlyUpdateProvidedFields_WhenPartialFieldsGiven()
    {
        // Arrange
        var repository = new FakeTravelItemRepository();
        var item = await repository.CreateAsync(new Domain.Entities.TravelItem
        {
            Name = "原始景點",
            Url = "https://example.com/original",
            Category = "OriginalCat"
        });

        var service = new EditService(repository);
        var request = new UpdateTravelItemRequest
        {
            Url = "https://example.com/only-url"
        };

        // Act
        var result = await service.UpdateAsync(item.Id, request);

        // Assert
        Assert.Equal(EditResultStatus.Success, result.Status);
        var updated = await repository.FindByIdAsync(item.Id);
        Assert.Equal("https://example.com/only-url", updated!.Url);
        Assert.Equal("原始景點", updated.Name);
        Assert.Equal("OriginalCat", updated.Category);
    }

    // 測試不修改名稱時，即使資料庫有同名項目也不會誤報重複
    [Fact]
    public async Task UpdateAsync_ShouldNotTriggerDuplicateCheck_WhenNameIsNotChanged()
    {
        // Arrange
        var repository = new FakeTravelItemRepository();
        var item1 = await repository.CreateAsync(new Domain.Entities.TravelItem
        {
            Name = "美麗海水族館",
            Url = "https://example.com/1",
            Category = "Attraction"
        });

        var service = new EditService(repository);
        var request = new UpdateTravelItemRequest
        {
            Category = "NewCategory"
        };

        // Act
        var result = await service.UpdateAsync(item1.Id, request);

        // Assert
        Assert.Equal(EditResultStatus.Success, result.Status);
        var updated = await repository.FindByIdAsync(item1.Id);
        Assert.Equal("NewCategory", updated!.Category);
        Assert.Equal("美麗海水族館", updated.Name);
    }
}

