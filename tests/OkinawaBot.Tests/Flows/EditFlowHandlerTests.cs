using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Flows;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;
using OkinawaBot.Domain.Entities;
using OkinawaBot.Tests.Fakes;

namespace OkinawaBot.Tests.Flows;

public class EditFlowHandlerTests
{
    private readonly ConversationStateManager _stateManager;
    private readonly BotCommandParser _commandParser;
    private readonly FakeTravelItemRepository _repository;
    private readonly EditService _editService;
    private readonly EditFlowHandler _handler;

    public EditFlowHandlerTests()
    {
        _stateManager = new ConversationStateManager();
        _commandParser = new BotCommandParser();
        _repository = new FakeTravelItemRepository();
        _editService = new EditService(_repository);

        _handler = new EditFlowHandler(
            _stateManager,
            _commandParser,
            _editService);
    }

    [Fact]
    public async Task Return_ShouldResetToMainMenu()
    {
        const string userId = "user-1";

        _stateManager.SetState(
            userId,
            ConversationState.EditItemSelection);

        var result = await _handler.HandleAsync(
            userId,
            "Return");

        var context =
            _stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.MainMenu,
            context.State);

        Assert.Equal(
            "已回到主選單。",
            result.Message);
    }

    [Fact]
    public async Task NoItems_ShouldResetToMainMenu()
    {
        const string userId = "user-1";

        _stateManager.SetState(
            userId,
            ConversationState.EditItemSelection);

        var result = await _handler.HandleAsync(
            userId,
            "1");

        var context =
            _stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.MainMenu,
            context.State);

        Assert.Equal(
            "目前沒有可以編輯的資料。",
            result.Message);
    }

    [Fact]
    public async Task InvalidSelection_ShouldStayInEditItemSelection()
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

        var result = await _handler.HandleAsync(
            userId,
            "abc");

        var context =
            _stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.EditItemSelection,
            context.State);

        Assert.Equal(
            "請輸入有效的項目編號。",
            result.Message);
    }

    [Fact]
    public async Task OutOfRangeSelection_ShouldStayInEditItemSelection()
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

        var result = await _handler.HandleAsync(
            userId,
            "2");

        var context =
            _stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.EditItemSelection,
            context.State);

        Assert.Equal(
            "請輸入 1 到 1 之間的項目編號。",
            result.Message);
    }

    [Fact]
    public async Task ValidSelection_ShouldMoveToEditDataInput()
    {
        const string userId = "user-1";

        var item = await _repository.CreateAsync(
            new TravelItem
            {
                Name = "美麗海水族館",
                Url = "https://example.com",
                Category = "Attraction"
            });

        _stateManager.SetState(
            userId,
            ConversationState.EditItemSelection);

        var result = await _handler.HandleAsync(
            userId,
            "1");

        var context =
            _stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.EditDataInput,
            context.State);

        Assert.Equal(
            item.Id,
            context.CurrentItemId);

        Assert.Contains(
            "美麗海水族館",
            result.Message);
    }
}
