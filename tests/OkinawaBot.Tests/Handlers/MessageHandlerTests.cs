using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Flows;
using OkinawaBot.Application.Handlers;
using OkinawaBot.Application.Input;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;
using OkinawaBot.Tests.Fakes;

namespace OkinawaBot.Tests.Handlers;

public class MessageHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldEnterSaveFlow_WhenUserSendsSave()
    {
        // Arrange
        var stateManager = new ConversationStateManager();
        var commandParser = new BotCommandParser();
        var inputParser = new SaveInputParser();

        var repository = new FakeTravelItemRepository();
        var saveService = new SaveService(repository);

        var saveFlowHandler = new SaveFlowHandler(stateManager, commandParser, inputParser, saveService);

        var handler = new MessageHandler( stateManager, commandParser, saveFlowHandler);

        var userId = "user-001";

        // Act
        await handler.HandleAsync(userId, "Save");

        // Assert
        var context = stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.SaveFlow,
            context.State);
    }

    [Fact]
    public async Task HandleAsync_ShouldSaveItem_WhenSaveInputIsValid()
    {
        // Arrange
        var stateManager = new ConversationStateManager();

        var commandParser = new BotCommandParser();

        var inputParser = new SaveInputParser();

        var repository = new FakeTravelItemRepository();

        var saveService = new SaveService(repository);

        var saveFlowHandler = new SaveFlowHandler(stateManager, commandParser, inputParser, saveService);

        var handler = new MessageHandler(stateManager, commandParser, saveFlowHandler);

        var userId = "user-001";

        // Act

        await handler.HandleAsync(
            userId,
            "Save");

        await handler.HandleAsync(
            userId,
            "https://example.com #ATTRACTION 美麗海水族館");

        // Assert

        var savedItem =
            repository.Items.Single();

        Assert.Equal(
            "美麗海水族館",
            savedItem.Name);

        Assert.Equal(
            "https://example.com",
            savedItem.Url);

        Assert.Equal(
            "ATTRACTION",
            savedItem.Category);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotSave_WhenInputIsInvalid()
    {
        // Arrange
        var stateManager = new ConversationStateManager();

        var commandParser = new BotCommandParser();

        var inputParser = new SaveInputParser();

        var repository = new FakeTravelItemRepository();

        var saveService = new SaveService(repository);

        var saveFlowHandler = new SaveFlowHandler(stateManager, commandParser, inputParser, saveService);

        var handler = new MessageHandler(stateManager, commandParser, saveFlowHandler);

        var userId = "user-001";

        // Act
        await handler.HandleAsync(userId, "Save");

        await handler.HandleAsync(
            userId,
            "https://example.com " +
            "美麗海水族館");

        // Assert
        Assert.Empty(repository.Items);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotCreateDuplicate_WhenNameAlreadyExists()
    {
        // Arrange
        var stateManager =
            new ConversationStateManager();

        var commandParser =
            new BotCommandParser();

        var inputParser =
            new SaveInputParser();

        var repository =
            new FakeTravelItemRepository();

        var saveService =
            new SaveService(repository);

        var saveFlowHandler = new SaveFlowHandler(stateManager, commandParser, inputParser, saveService);

        var handler = new MessageHandler(stateManager, commandParser, saveFlowHandler);

        var userId = "user-001";

        // Act

        await handler.HandleAsync(
            userId,
            "Save");

        await handler.HandleAsync(
            userId,
            "https://example.com #ATTRACTION 美麗海水族館");

        await handler.HandleAsync(
            userId,
            "Save");

        await handler.HandleAsync(
            userId,
            "https://example.com/2 #ATTRACTION 美麗海水族館");

        // Assert

        Assert.Single(repository.Items);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnToMainMenu_WhenUserEntersReturn()
    {
        // Arrange
        var stateManager =
            new ConversationStateManager();

        var commandParser =
            new BotCommandParser();

        var inputParser =
            new SaveInputParser();

        var repository =
            new FakeTravelItemRepository();

        var saveService =
            new SaveService(repository);

        var saveFlowHandler = new SaveFlowHandler(stateManager, commandParser, inputParser, saveService);

        var handler = new MessageHandler(stateManager, commandParser, saveFlowHandler);

        var userId = "user-001";

        // Act

        await handler.HandleAsync(
            userId,
            "Save");

        await handler.HandleAsync(
            userId,
            "return");

        // Assert

        var context =
            stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.MainMenu,
            context.State);
    }

    //進入 Save 時應回覆輸入格式 (AC-003-01)
    [Fact]
    public async Task HandleAsync_ShouldReturnSaveFormat_WhenUserSendsSave()
    {
        // Arrange
        var handler = CreateHandler(out _, out _);

        // Act
        var response =
            await handler.HandleAsync(
                "user-001",
                "Save");

        // Assert
        Assert.Contains(
            "URL #分類 名稱",
            response.Message);
    }

    //SaveFlowHandler 的回覆應該傳遞到 MessageHandler
    [Fact]
    public async Task HandleAsync_ShouldReturnSaveFlowResponse_WhenSaveSucceeds()
    {
        // Arrange
        var handler = CreateHandler(out _, out _);

        var userId = "user-001";

        await handler.HandleAsync(
            userId,
            "Save");

        // Act
        var response =
            await handler.HandleAsync(
                userId,
                "https://example.com #ATTRACTION 美麗海水族館");

        // Assert
        Assert.Equal(
            "旅遊資訊儲存成功。",
            response.Message);
    }

    //主選單收到無法辨識的指令時，應回覆可用指令
    [Fact]
    public async Task HandleAsync_ShouldReturnMainMenu_WhenCommandIsUnknown()
    {
        // Arrange
        var handler = CreateHandler(out var stateManager, out _);

        var userId = "user-001";

        // Act
        var response =
            await handler.HandleAsync(
                userId,
                "hello");

        // Assert
        Assert.Contains(
            "Save",
            response.Message);

        Assert.Equal(
            ConversationState.MainMenu,
            stateManager.GetOrCreate(userId).State);
    }

    //尚未實作的流程應可透過 Return 回到主選單，不會卡住
    [Theory]
    [InlineData("Query")]
    [InlineData("Edit")]
    [InlineData("Delete")]
    public async Task HandleAsync_ShouldReturnToMainMenu_WhenUserEntersReturnInNotAvailableFlow(
        string command)
    {
        // Arrange
        var handler = CreateHandler(out var stateManager, out _);

        var userId = "user-001";

        var enterResponse =
            await handler.HandleAsync(
                userId,
                command);

        // Act
        var response =
            await handler.HandleAsync(
                userId,
                "Return");

        // Assert
        Assert.Contains(
            "尚未開放",
            enterResponse.Message);

        Assert.Equal(
            "已回到主選單。",
            response.Message);

        Assert.Equal(
            ConversationState.MainMenu,
            stateManager.GetOrCreate(userId).State);
    }

    private static MessageHandler CreateHandler(
        out ConversationStateManager stateManager,
        out FakeTravelItemRepository repository)
    {
        stateManager = new ConversationStateManager();

        var commandParser = new BotCommandParser();

        var inputParser = new SaveInputParser();

        repository = new FakeTravelItemRepository();

        var saveService = new SaveService(repository);

        var saveFlowHandler = new SaveFlowHandler(stateManager, commandParser, inputParser, saveService);

        return new MessageHandler(stateManager, commandParser, saveFlowHandler);
    }
}