using System.Threading;
using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Flows;
using OkinawaBot.Application.Input;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;
using OkinawaBot.Tests.Fakes;
using Xunit;

namespace OkinawaBot.Tests.Flows;

public class SaveFlowHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnToMainMenu_WhenUserEntersReturn()
    {
        // Arrange
        var userId = "user-1";

        var stateManager =
            new ConversationStateManager(new FakeDistributedCache());

        var commandParser =
            new BotCommandParser();

        var inputParser =
            new SaveInputParser();

        var repository =
            new FakeTravelItemRepository();

        var saveService =
            new SaveService(repository);

        var handler =
            new SaveFlowHandler(
                stateManager,
                commandParser,
                inputParser,
                saveService);

        stateManager.SetState(
            userId,
            ConversationState.SaveFlow);

        // Act
        var response =
            await handler.Handle(new OkinawaBot.Application.Requests.SaveFlowCommand(userId, "Return"), CancellationToken.None);

        // Assert
        var context =
            stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.Idle,
            context.State);

        Assert.Equal(
            "已回到主選單。",
            response.Message);
    }


    //模擬儲存成功且回MAINMENU
    [Fact]
    public async Task HandleAsync_ShouldSaveItemAndReturnToMainMenu_WhenInputIsValid()
    {
        // Arrange
        var userId = "user-1";

        var stateManager =
            new ConversationStateManager(new FakeDistributedCache());

        var commandParser =
            new BotCommandParser();

        var inputParser =
            new SaveInputParser();

        var repository =
            new FakeTravelItemRepository();

        var saveService =
            new SaveService(repository);

        var handler =
            new SaveFlowHandler(
                stateManager,
                commandParser,
                inputParser,
                saveService);

        stateManager.SetState(
            userId,
            ConversationState.SaveFlow);

        //C# 原始字串寫法 => 模擬使用者輸入情境
        var input =
        """
        https://example.com/okinawa
        #Attraction
        美麗海水族館
        """;

        // Act
        var response =
            await handler.Handle(new OkinawaBot.Application.Requests.SaveFlowCommand(userId, input), CancellationToken.None);

        // Assert
        var context =
            stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.Idle,
            context.State);

        Assert.Equal(
            "旅遊資訊儲存成功。",
            response.Message);
    }

    //存取失敗不會回到menu(不會經過saveService)
    [Fact]
    public async Task HandleAsync_ShouldStayInSaveFlow_WhenInputIsInvalid()
    {
        // Arrange
        var userId = "user-1";

        var stateManager =
            new ConversationStateManager(new FakeDistributedCache());

        var commandParser =
            new BotCommandParser();

        var inputParser =
            new SaveInputParser();

        var repository =
            new FakeTravelItemRepository();

        var saveService =
            new SaveService(repository);

        var handler =
            new SaveFlowHandler(
                stateManager,
                commandParser,
                inputParser,
                saveService);

        stateManager.SetState(
            userId,
            ConversationState.SaveFlow);

        var input =
            """
        https://example.com/okinawa
        美麗海水族館
        """;

        // Act
        var response =
            await handler.Handle(new OkinawaBot.Application.Requests.SaveFlowCommand(userId, input), CancellationToken.None);

        // Assert
        var context =
            stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.SaveFlow,
            context.State);

        Assert.Equal(
            "缺少分類，請使用 #分類 格式，例如 #Attraction。",
            response.Message);
    }

    //測試當使用者 Save 一個已存在 Name 的資料時，不應該直接儲存，而是進入確認狀態。
    [Fact]
    public async Task HandleAsync_ShouldAskForConfirmation_WhenNameAlreadyExists()
    {
        // Arrange
        var userId = "user-1";

        var stateManager = new ConversationStateManager(new FakeDistributedCache());
        var commandParser = new BotCommandParser();
        var inputParser = new SaveInputParser();
        var repository = new FakeTravelItemRepository();
        var saveService = new SaveService(repository);

        var handler = new SaveFlowHandler(
            stateManager,
            commandParser,
            inputParser,
            saveService);

        stateManager.SetState(
            userId,
            ConversationState.SaveFlow);

        var existingRequest =
            new SaveTravelItemRequest
            {
                Url = "https://example.com/old",
                Category = "Attraction",
                Name = "美麗海水族館"
            };

        await saveService.SaveAsync(existingRequest);

        var input =
            """
        https://example.com/new
        #Attraction
        美麗海水族館
        """;

        // Act
        var response =
            await handler.Handle(new OkinawaBot.Application.Requests.SaveFlowCommand(userId, input), CancellationToken.None);

        // Assert
        var context =
            stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.SaveDuplicateConfirmation,
            context.State);

        Assert.Equal(
            "美麗海水族館",
            context.PendingSave!.Name);

        Assert.Contains(
            "已經存在",
            response.Message);
    }

    //測試重複存取
    [Fact]
    public async Task HandleAsync_ShouldSaveDuplicate_WhenUserEntersContinue()
    {
        // Arrange
        var userId = "user-1";

        var stateManager = new ConversationStateManager(new FakeDistributedCache());
        var commandParser = new BotCommandParser();
        var inputParser = new SaveInputParser();
        var repository = new FakeTravelItemRepository();
        var saveService = new SaveService(repository);

        var handler = new SaveFlowHandler(
            stateManager,
            commandParser,
            inputParser,
            saveService);

        stateManager.SetState(
            userId,
            ConversationState.SaveFlow);

        var existingRequest =
            new SaveTravelItemRequest
            {
                Url = "https://example.com/old",
                Category = "Attraction",
                Name = "美麗海水族館"
            };

        await saveService.SaveAsync(existingRequest);

        var input =
            """
        https://example.com/new
        #Attraction
        美麗海水族館
        """;

        await handler.Handle(new OkinawaBot.Application.Requests.SaveFlowCommand(userId, input), CancellationToken.None);

        // Act
        var response =
            await handler.Handle(new OkinawaBot.Application.Requests.SaveFlowCommand(userId, "Continue"), CancellationToken.None);

        // Assert
        var context =
            stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.Idle,
            context.State);

        Assert.Equal(
            "旅遊資訊儲存成功。",
            response.Message);

        Assert.Equal(
            2,
            repository.Items.Count);
    }

    //測不重複存取(return)
    [Fact]
    public async Task HandleAsync_ShouldCancelDuplicateSave_WhenUserEntersReturn()
    {
        // Arrange
        var userId = "user-1";

        var stateManager = new ConversationStateManager(new FakeDistributedCache());
        var commandParser = new BotCommandParser();
        var inputParser = new SaveInputParser();
        var repository = new FakeTravelItemRepository();
        var saveService = new SaveService(repository);

        var handler = new SaveFlowHandler(
            stateManager,
            commandParser,
            inputParser,
            saveService);

        stateManager.SetState(
            userId,
            ConversationState.SaveFlow);

        var existingRequest =
            new SaveTravelItemRequest
            {
                Url = "https://example.com/old",
                Category = "Attraction",
                Name = "美麗海水族館"
            };

        await saveService.SaveAsync(existingRequest);

        var input =
            """
        https://example.com/new
        #Attraction
        美麗海水族館
        """;

        await handler.Handle(new OkinawaBot.Application.Requests.SaveFlowCommand(userId, input), CancellationToken.None);

        // Act
        var response =
            await handler.Handle(new OkinawaBot.Application.Requests.SaveFlowCommand(userId, "Return"), CancellationToken.None);

        // Assert
        var context =
            stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.Idle,
            context.State);

        Assert.Null(
            context.PendingSave);

        Assert.Equal(
            "已取消儲存並回到主選單。",
            response.Message);

        Assert.Single(repository.Items);
    }

}