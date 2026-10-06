using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Flows;
using OkinawaBot.Application.Handlers;
using OkinawaBot.Application.Input;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;
using OkinawaBot.Domain.Entities;
using OkinawaBot.Tests.Fakes;

namespace OkinawaBot.Tests.Handlers;

public class MessageHandlerTests
{
    private readonly ConversationStateManager _stateManager = new();
    private readonly FakeTravelItemRepository _repository = new();
    private readonly MessageHandler _handler;

    public MessageHandlerTests()
    {
        var commandParser = new BotCommandParser();
        var inputParser = new SaveInputParser();
        var saveService = new SaveService(_repository);
        var queryService = new QueryService(_repository);
        var editService = new EditService(_repository);
        var deleteService = new DeleteService(_repository);

        var saveFlowHandler = new SaveFlowHandler(_stateManager, commandParser, inputParser, saveService);
        var queryFlowHandler = new QueryFlowHandler(_stateManager, commandParser, queryService);
        var editFlowHandler = new EditFlowHandler(_stateManager, commandParser, editService, inputParser);
        var deleteFlowHandler = new DeleteFlowHandler(_stateManager, commandParser, deleteService);

        _handler = new MessageHandler(_stateManager, commandParser, saveFlowHandler, queryFlowHandler, editFlowHandler, deleteFlowHandler);
    }

    // 驗證收到 Save 指令時，會正確切換至 SaveFlow 狀態
    [Fact]
    public async Task HandleAsync_ShouldEnterSaveFlow_WhenUserSendsSave()
    {
        // Arrange
        const string userId = "user-001";

        // Act
        await _handler.HandleAsync(userId, "Save");

        // Assert
        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.SaveFlow, context.State);
    }

    // 驗證收到 Query 指令時，會正確切換至 QueryFlow 狀態
    [Fact]
    public async Task HandleAsync_ShouldEnterQueryFlow_WhenUserSendsQuery()
    {
        // Arrange
        const string userId = "user-001";

        // Act
        await _handler.HandleAsync(userId, "Query");

        // Assert
        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.QueryFlow, context.State);
    }

    // 驗證在 SaveFlow 下輸入有效格式時，能順利儲存旅遊資訊
    [Fact]
    public async Task HandleAsync_ShouldSaveItem_WhenSaveInputIsValid()
    {
        // Arrange
        const string userId = "user-001";

        // Act
        await _handler.HandleAsync(userId, "Save");
        await _handler.HandleAsync(userId, "https://example.com #ATTRACTION 美麗海水族館");

        // Assert
        var savedItem = Assert.Single(_repository.Items);
        Assert.Equal("美麗海水族館", savedItem.Name);
        Assert.Equal("https://example.com", savedItem.Url);
        Assert.Equal("ATTRACTION", savedItem.Category);
    }

    // 驗證輸入格式無效時，不會儲存任何資料
    [Fact]
    public async Task HandleAsync_ShouldNotSave_WhenInputIsInvalid()
    {
        // Arrange
        const string userId = "user-001";

        // Act
        await _handler.HandleAsync(userId, "Save");
        await _handler.HandleAsync(userId, "https://example.com 美麗海水族館");

        // Assert
        Assert.Empty(_repository.Items);
    }

    // 驗證名稱已存在時，不會建立重複的資料
    [Fact]
    public async Task HandleAsync_ShouldNotCreateDuplicate_WhenNameAlreadyExists()
    {
        // Arrange
        const string userId = "user-001";

        // Act
        await _handler.HandleAsync(userId, "Save");
        await _handler.HandleAsync(userId, "https://example.com #ATTRACTION 美麗海水族館");

        await _handler.HandleAsync(userId, "Save");
        await _handler.HandleAsync(userId, "https://example.com/2 #ATTRACTION 美麗海水族館");

        // Assert
        Assert.Single(_repository.Items);
    }

    // 驗證輸入 Return 時，能正確返回 MainMenu
    [Fact]
    public async Task HandleAsync_ShouldReturnToMainMenu_WhenUserEntersReturn()
    {
        // Arrange
        const string userId = "user-001";

        // Act
        await _handler.HandleAsync(userId, "Save");
        await _handler.HandleAsync(userId, "return");

        // Assert
        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.MainMenu, context.State);
    }

    // MessageHandler 是否真的把訊息交給 EditFlowHandler。
    [Fact]
    public async Task EditCommand_ShouldEnterEditItemSelection()
    {
        const string userId = "user-1";

        var result = await _handler.HandleAsync(
            userId,
            "Edit");

        var context =
            _stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.EditItemSelection,
            context.State);
    }

    // 測 Edit Flow 的實際 routing
    [Fact]
    public async Task EditState_ShouldRouteToEditFlowHandler()
    {
        const string userId = "user-1";

        await _repository.CreateAsync(
            new TravelItem
            {
                Name = "美麗海水族館",
                Url = "https://example.com",
                Category = "Attraction"
            });

        _stateManager.SetState(
            userId,
            ConversationState.EditItemSelection);

        var result =
            await _handler.HandleAsync(
                userId,
                "1");

        var context =
            _stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.EditDataInput,
            context.State);

        Assert.Equal(
            1,
            context.CurrentItemId);

        Assert.Contains(
            "美麗海水族館",
            result.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteCommand_ShouldEnterDeleteFlow()
    {
        // Act
        var response =
            await _handler.HandleAsync(
                "test-user",
                "Delete");

        // Assert
        var context =
            _stateManager.GetOrCreate("test-user");

        Assert.Equal(
            ConversationState.DeleteFlow,
            context.State);
    }

    [Fact]
    public async Task HandleAsync_WhenInDeleteFlowAndReturn_ShouldGoBackToMainMenu()
    {
        // Arrange
        await _handler.HandleAsync(
            "test-user",
            "Delete");

        // Act
        var response =
            await _handler.HandleAsync(
                "test-user",
                "Return");

        // Assert
        var context =
            _stateManager.GetOrCreate("test-user");

        Assert.Equal(
            ConversationState.MainMenu,
            context.State);
    }
}