using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Flows;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;
using OkinawaBot.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OkinawaBot.Tests.Flows;

public class QueryFlowHandlerTests
{
    //測試進入查詢模式後返回
    [Fact]
    public async Task HandleAsync_ShouldReturnToMainMenu_WhenUserEntersReturn()
    {
        // Arrange
        var userId = "user-1";

        var stateManager =
            new ConversationStateManager();

        var commandParser =
            new BotCommandParser();

        var repository =
            new FakeTravelItemRepository();

        var queryService =
            new QueryService(repository);

        var handler =
            new QueryFlowHandler(
                stateManager,
                commandParser,
                queryService);

        stateManager.SetState(
            userId,
            ConversationState.QueryFlow);

        // Act
        var response =
            await handler.HandleAsync(
                userId,
                "Return");

        // Assert
        var context =
            stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.MainMenu,
            context.State);

        Assert.Equal(
            "已回到主選單。",
            response.Message);
    }

    //測試無類別資料之結果
    [Fact]
    public async Task HandleAsync_ShouldStayInQueryFlow_WhenCategoryHasNoItems()
    {
        // Arrange
        var userId = "user-1";

        var stateManager =
            new ConversationStateManager();

        var commandParser =
            new BotCommandParser();

        var repository =
            new FakeTravelItemRepository();

        var queryService =
            new QueryService(repository);

        var handler =
            new QueryFlowHandler(
                stateManager,
                commandParser,
                queryService);

        stateManager.SetState(
            userId,
            ConversationState.QueryFlow);

        // Act
        var response =
            await handler.HandleAsync(
                userId,
                "Restaurant");

        // Assert
        var context =
            stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.QueryFlow,
            context.State);

        Assert.Contains(
            "找不到類別",
            response.Message);
    }

    //測試成功查詢到類別
    [Fact]
    public async Task HandleAsync_ShouldReturnItemsAndMainMenu_WhenCategoryHasItems()
    {
        // Arrange
        var userId = "user-1";

        var stateManager =
            new ConversationStateManager();

        var commandParser =
            new BotCommandParser();

        var repository =
            new FakeTravelItemRepository();

        //先走save存入資料
        var saveService =
            new SaveService(repository);

        await saveService.SaveAsync(
            new SaveTravelItemRequest
            {
                Url = "https://example.com/churaumi",
                Category = "Attraction",
                Name = "美麗海水族館"
            });

        await saveService.SaveAsync(
            new SaveTravelItemRequest
            {
                Url = "https://example.com/kouri",
                Category = "Attraction",
                Name = "古宇利島"
            });

        var queryService =
            new QueryService(repository);

        var handler =
            new QueryFlowHandler(
                stateManager,
                commandParser,
                queryService);

        stateManager.SetState(
            userId,
            ConversationState.QueryFlow);

        // Act
        var response =
            await handler.HandleAsync(
                userId,
                "Attraction");

        // Assert
        var context =
            stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.MainMenu,
            context.State);

        Assert.Contains(
            "美麗海水族館",
            response.Message);

        Assert.Contains(
            "古宇利島",
            response.Message);

        Assert.Contains(
            "https://example.com/churaumi",
            response.Message);

        Assert.Contains(
            "https://example.com/kouri",
            response.Message);
    }
}
