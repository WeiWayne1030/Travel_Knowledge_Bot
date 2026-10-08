using System.Threading;
using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Flows;
using OkinawaBot.Application.Input;
using OkinawaBot.Application.Models;
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
    private readonly SaveInputParser _inputParser;

    public EditFlowHandlerTests()
    {
        _stateManager = new ConversationStateManager(new FakeDistributedCache());
        _commandParser = new BotCommandParser();
        _repository = new FakeTravelItemRepository();
        _editService = new EditService(_repository);
        _inputParser = new SaveInputParser();

        _handler = new EditFlowHandler(
            _stateManager,
            _commandParser,
            _editService,
            _inputParser);
    }

    [Fact]
    public async Task Return_ShouldResetToMainMenu()
    {
        const string userId = "user-1";

        _stateManager.SetState(
            userId,
            ConversationState.EditItemSelection);

        var result = await _handler.Handle(new OkinawaBot.Application.Requests.EditFlowCommand(userId, "返回主選單"), CancellationToken.None);

        var context =
            _stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.Idle,
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

        var result = await _handler.Handle(new OkinawaBot.Application.Requests.EditFlowCommand(userId, "1"), CancellationToken.None);

        var context =
            _stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.Idle,
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

        var result = await _handler.Handle(new OkinawaBot.Application.Requests.EditFlowCommand(userId, "abc"), CancellationToken.None);

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

        var result = await _handler.Handle(new OkinawaBot.Application.Requests.EditFlowCommand(userId, "2"), CancellationToken.None);

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

        var result = await _handler.Handle(new OkinawaBot.Application.Requests.EditFlowCommand(userId, "1"), CancellationToken.None);

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

    //不存在currentItemId
    [Fact]
    public async Task DataInput_WithoutCurrentItemId_ShouldReset()
    {
        const string userId = "user-1";

        _stateManager.SetState(
            userId,
            ConversationState.EditDataInput);

        var result = await _handler.Handle(new OkinawaBot.Application.Requests.EditFlowCommand(userId, """
        https://example.com
        #Attraction
        美麗海水族館
        """), CancellationToken.None);

        var context =
            _stateManager.GetOrCreate(userId);

        Assert.Equal(
            ConversationState.Idle,
            context.State);

        Assert.Equal(
            "找不到目前要編輯的資料，請重新操作。",
            result.Message);
    }

    //格式輸入錯誤
    [Fact]
    public async Task InvalidData_ShouldStayInEditDataInput()
    {
        const string userId = "user-1";

        var item = await _repository.CreateAsync(
            new TravelItem
            {
                Name = "美麗海水族館",
                Url = "https://example.com",
                Category = "Attraction"
            });

        var context =
            _stateManager.GetOrCreate(userId);

        context.State =
            ConversationState.EditDataInput;

        context.CurrentItemId =
            item.Id;

        var result = await _handler.Handle(new OkinawaBot.Application.Requests.EditFlowCommand(userId, "輸入錯誤"), CancellationToken.None);

        Assert.Equal(
            ConversationState.EditDataInput,
            context.State);

        Assert.Contains(
            "缺少分類",
            result.Message);
    }

    //正常修改流程
    [Fact]
    public async Task ValidData_ShouldUpdateItemAndReturnToMainMenu()
    {
        const string userId = "user-1";

        var item = await _repository.CreateAsync(
            new TravelItem
            {
                Name = "舊名稱",
                Url = "https://old.example.com",
                Category = "OldCategory"
            });

        var context =
            _stateManager.GetOrCreate(userId);

        context.State =
            ConversationState.EditDataInput;

        context.CurrentItemId =
            item.Id;

        var result = await _handler.Handle(new OkinawaBot.Application.Requests.EditFlowCommand(userId, """
        https://new.example.com
        #Attraction
        美麗海水族館
        """), CancellationToken.None);

        var updatedItem =
            await _repository.FindByIdAsync(item.Id);

        Assert.Equal(
            ConversationState.Idle,
            context.State);

        Assert.Equal(
            "美麗海水族館",
            updatedItem!.Name);

        Assert.Equal(
            "https://new.example.com",
            updatedItem.Url);

        Assert.Equal(
            "Attraction",
            updatedItem.Category);

        Assert.Equal(
            "旅遊資訊修改成功。",
            result.Message);
    }

    //測試重複資訊
    [Fact]
    public async Task DuplicateName_ShouldEnterDuplicateConfirmation()
    {
        const string userId = "user-1";

        var firstItem = await _repository.CreateAsync(
            new TravelItem
            {
                Name = "美麗海水族館",
                Url = "https://aquarium.example.com",
                Category = "Attraction"
            });

        var secondItem = await _repository.CreateAsync(
            new TravelItem
            {
                Name = "首里城",
                Url = "https://shuri.example.com",
                Category = "Attraction"
            });

        var context =
            _stateManager.GetOrCreate(userId);

        context.State =
            ConversationState.EditDataInput;

        context.CurrentItemId =
            secondItem.Id;

        var result = await _handler.Handle(new OkinawaBot.Application.Requests.EditFlowCommand(userId, """
        https://new.example.com
        #Attraction
        美麗海水族館
        """), CancellationToken.None);

        Assert.Equal(
            ConversationState.EditDuplicateConfirmation,
            context.State);

        Assert.Equal(
            secondItem.Id,
            context.CurrentItemId);

        Assert.Contains(
            "已經存在",
            result.Message);
    }


    //return
    [Fact]
    public async Task DuplicateConfirmation_Return_ShouldCancelEdit()
    {
        const string userId = "user-1";

        var item = await _repository.CreateAsync(
            new TravelItem
            {
                Name = "首里城",
                Url = "https://shuri.example.com",
                Category = "Attraction"
            });

        var context =
            _stateManager.GetOrCreate(userId);

        context.State =
            ConversationState.EditDuplicateConfirmation;

        context.CurrentItemId =
            item.Id;

        context.PendingEdit =
            new UpdateTravelItemRequest
            {
                Name = "美麗海水族館",
                Url = "https://example.com",
                Category = "Attraction"
            };

        var result = await _handler.Handle(new OkinawaBot.Application.Requests.EditFlowCommand(userId, "返回主選單"), CancellationToken.None);

        Assert.Equal(
            ConversationState.Idle,
            context.State);

        Assert.Null(context.CurrentItemId);

        Assert.Null(context.PendingEdit);

        Assert.Equal(
            "已回到主選單。",
            result.Message);
    }

    //Continue Test
    [Fact]
    public async Task DuplicateConfirmation_Continue_ShouldUpdateItem()
    {
        const string userId = "user-1";

        await _repository.CreateAsync(
            new TravelItem
            {
                Name = "美麗海水族館",
                Url = "https://aquarium.example.com",
                Category = "Attraction"
            });

        var item = await _repository.CreateAsync(
            new TravelItem
            {
                Name = "首里城",
                Url = "https://shuri.example.com",
                Category = "Attraction"
            });

        var context =
            _stateManager.GetOrCreate(userId);

        context.State =
            ConversationState.EditDuplicateConfirmation;

        context.CurrentItemId =
            item.Id;

        context.PendingEdit =
            new UpdateTravelItemRequest
            {
                Name = "美麗海水族館",
                Url = "https://new.example.com",
                Category = "Restaurant"
            };

        var result = await _handler.Handle(new OkinawaBot.Application.Requests.EditFlowCommand(userId, "繼續"), CancellationToken.None);

        var updatedItem =
            await _repository.FindByIdAsync(item.Id);

        Assert.Equal(
            ConversationState.Idle,
            context.State);

        Assert.Equal(
            "美麗海水族館",
            updatedItem!.Name);

        Assert.Equal(
            "https://new.example.com",
            updatedItem.Url);

        Assert.Equal(
            "Restaurant",
            updatedItem.Category);

        Assert.Null(context.PendingEdit);

        Assert.Equal(
            "旅遊資訊修改成功。",
            result.Message);
    }
}
