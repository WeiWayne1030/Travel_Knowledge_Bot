using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Flows;
using OkinawaBot.Application.Handlers;
using OkinawaBot.Application.Input;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Requests;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;
using OkinawaBot.Domain.Entities;
using OkinawaBot.Domain.Interfaces;
using OkinawaBot.Tests.Fakes;

namespace OkinawaBot.Tests.Handlers;

public class MessageHandlerTests
{
    private readonly ConversationStateManager _stateManager;
    private readonly FakeTravelItemRepository _repository;
    private readonly ProcessMessageCommandHandler _handler;
    private readonly IMediator _mediator;

    public MessageHandlerTests()
    {
        _stateManager = new ConversationStateManager(new FakeDistributedCache());
        _repository = new FakeTravelItemRepository();

        var services = new ServiceCollection();

        // 註冊所有必要的 Services
        services.AddSingleton(_stateManager);
        services.AddSingleton<ITravelItemRepository>(_repository);
        services.AddScoped<BotCommandParser>();
        services.AddScoped<SaveInputParser>();
        services.AddScoped<EditInputParser>();
        services.AddScoped<SaveService>();
        services.AddScoped<QueryService>();
        services.AddScoped<EditService>();
        services.AddScoped<DeleteService>();
        
        // 註冊 MediatR (包含所有的 Flow Handlers)
        services.AddLogging();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProcessMessageCommand).Assembly));

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();

        // 為了測試，直接建立我們要測的 Handler
        _handler = new ProcessMessageCommandHandler(
            _stateManager,
            provider.GetRequiredService<BotCommandParser>(),
            _mediator,
            provider.GetRequiredService<QueryService>());
    }

    private Task<BotResponse> HandleAsync(string userId, string message)
    {
        return _handler.Handle(new ProcessMessageCommand(userId, message), CancellationToken.None);
    }

    // 驗證收到 Save 指令時，會正確切換至 SaveFlow 狀態
    [Fact]
    public async Task HandleAsync_ShouldEnterSaveFlow_WhenUserSendsSave()
    {
        const string userId = "user-001";
        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "儲存");

        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.SaveFlow, context.State);
    }

    // 驗證收到 Query 指令時，會正確切換至 QueryFlow 狀態
    [Fact]
    public async Task HandleAsync_ShouldEnterQueryFlow_WhenUserSendsQuery()
    {
        const string userId = "user-001";
        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "查詢");

        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.QueryFlow, context.State);
    }

    // 驗證在 SaveFlow 下輸入有效格式時，能順利儲存旅遊資訊
    [Fact]
    public async Task HandleAsync_ShouldSaveItem_WhenSaveInputIsValid()
    {
        const string userId = "user-001";

        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "儲存");
        await HandleAsync(userId, "https://example.com #ATTRACTION 美麗海水族館");

        var savedItem = Assert.Single(_repository.Items);
        Assert.Equal("美麗海水族館", savedItem.Name);
        Assert.Equal("https://example.com", savedItem.Url);
        Assert.Equal("ATTRACTION", savedItem.Category);
    }

    // 驗證輸入格式無效時，不會儲存任何資料
    [Fact]
    public async Task HandleAsync_ShouldNotSave_WhenInputIsInvalid()
    {
        const string userId = "user-001";

        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "儲存");
        await HandleAsync(userId, "https://example.com 美麗海水族館");

        Assert.Empty(_repository.Items);
    }

    // 驗證名稱已存在時，不會建立重複的資料
    [Fact]
    public async Task HandleAsync_ShouldNotCreateDuplicate_WhenNameAlreadyExists()
    {
        const string userId = "user-001";

        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "儲存");
        await HandleAsync(userId, "https://example.com #ATTRACTION 美麗海水族館");

        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "儲存");
        await HandleAsync(userId, "https://example.com/2 #ATTRACTION 美麗海水族館");

        Assert.Single(_repository.Items);
    }

    // 驗證輸入 Return 時，能正確返回 MainMenu
    [Fact]
    public async Task HandleAsync_ShouldReturnToMainMenu_WhenUserEntersReturn()
    {
        const string userId = "user-001";

        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "儲存");
        await HandleAsync(userId, "返回主選單");

        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.MainMenu, context.State);
    }

    // 驗證輸入 再見小幫手 時，能正確返回 Idle (退出小幫手)
    [Fact]
    public async Task HandleAsync_ShouldReturnToIdle_WhenUserEntersSleep()
    {
        const string userId = "user-sleep";

        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "再見小幫手");

        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.Idle, context.State);
    }

    // MessageHandler 是否真的把訊息交給 EditFlowHandler。
    [Fact]
    public async Task EditCommand_ShouldEnterEditItemSelection()
    {
        const string userId = "user-1";

        await _repository.CreateAsync(new TravelItem
        {
            Destination = "沖繩",
            Name = "美麗海水族館",
            Url = "https://example.com",
            Category = "Attraction"
        });

        await HandleAsync(userId, "旅遊小幫手");
        var response = await HandleAsync(userId, "編輯");

        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.EditItemSelection, context.State);
        Assert.Contains("目前可編輯的項目：", response.Message);
        Assert.Contains("美麗海水族館", response.Message);
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

        _stateManager.SetState(userId, ConversationState.EditItemSelection);

        await HandleAsync(userId, "1");

        var context = _stateManager.GetOrCreate(userId);

        Assert.Equal(ConversationState.EditDataInput, context.State);
        Assert.Equal(1, context.CurrentItemId);
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteCommand_ShouldEnterDeleteFlow()
    {
        const string userId = "test-user";

        await _repository.CreateAsync(new TravelItem
        {
            Destination = "沖繩",
            Name = "波上宮",
            Url = "https://example.com/naminoue",
            Category = "Attraction"
        });

        await HandleAsync(userId, "旅遊小幫手");
        var response = await HandleAsync(userId, "刪除");

        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.DeleteFlow, context.State);
        Assert.Contains("目前可刪除的項目：", response.Message);
        Assert.Contains("波上宮", response.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenInDeleteFlowAndReturn_ShouldGoBackToMainMenu()
    {
        await HandleAsync("test-user", "旅遊小幫手");
        await HandleAsync("test-user", "刪除");
        await HandleAsync("test-user", "返回主選單");

        var context = _stateManager.GetOrCreate("test-user");
        Assert.Equal(ConversationState.MainMenu, context.State);
    }

    // 驗證任何 Flow 狀態下輸入「返回主選單」，皆會清除暫存並回到 MainMenu
    [Theory]
    [InlineData(ConversationState.SaveFlow)]
    [InlineData(ConversationState.SaveDuplicateConfirmation)]
    [InlineData(ConversationState.QueryFlow)]
    [InlineData(ConversationState.EditItemSelection)]
    [InlineData(ConversationState.EditDataInput)]
    [InlineData(ConversationState.EditDuplicateConfirmation)]
    [InlineData(ConversationState.DeleteFlow)]
    [InlineData(ConversationState.DeleteConfirmation)]
    [InlineData(ConversationState.SwitchDestination)]
    public async Task HandleAsync_ReturnFromAnyFlow_ShouldResetAndGoToMainMenu(ConversationState state)
    {
        const string userId = "user-return";

        var context = _stateManager.GetOrCreate(userId);
        context.State = state;
        context.CurrentItemId = 1;
        context.PendingSave = new OkinawaBot.Application.Models.SaveTravelItemRequest
        {
            Url = "https://example.com", Category = "Attraction", Name = "美麗海水族館"
        };
        context.PendingEdit = new OkinawaBot.Application.Models.UpdateTravelItemRequest
        {
            Url = "https://example.com", Category = "Attraction", Name = "美麗海水族館"
        };

        var response = await _handler.Handle(
            new ProcessMessageCommand(userId, "返回主選單"), CancellationToken.None);

        Assert.Equal(ConversationState.MainMenu, context.State);
        Assert.Null(context.CurrentItemId);
        Assert.Null(context.PendingSave);
        Assert.Null(context.PendingEdit);
        Assert.Contains("旅遊小幫手", response.Message);
        Assert.Empty(_repository.Items);
    }

    // 驗證輸入「切換地區」會進入 SwitchDestination 狀態
    [Fact]
    public async Task HandleAsync_WhenSwitchDestinationCommand_ShouldEnterSwitchDestinationState()
    {
        const string userId = "user-switch";
        await HandleAsync(userId, "旅遊小幫手");
        var response = await HandleAsync(userId, "切換地區");

        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.SwitchDestination, context.State);
        Assert.Contains("目前選定地區為【沖繩】", response.Message);
    }

    // 驗證輸入新地區名稱時能成功切換地區並回到主選單
    [Fact]
    public async Task HandleAsync_WhenInSwitchDestinationState_ShouldChangeCurrentDestination()
    {
        const string userId = "user-switch-tokyo";
        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "切換地區");
        var response = await HandleAsync(userId, "東京");

        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.MainMenu, context.State);
        Assert.Equal("東京", context.CurrentDestination);
        Assert.Contains("已切換目前地區為【東京】", response.Message);
    }

    // 驗證切換地區後儲存的項目歸屬於該地區
    [Fact]
    public async Task HandleAsync_SaveItem_ShouldSaveUnderCurrentDestination()
    {
        const string userId = "user-dest-save";
        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "切換地區");
        await HandleAsync(userId, "京都");

        await HandleAsync(userId, "儲存");
        await HandleAsync(userId, "https://kyoto.example.com #Attraction 金閣寺");

        var savedItem = Assert.Single(_repository.Items);
        Assert.Equal("京都", savedItem.Destination);
        Assert.Equal("金閣寺", savedItem.Name);
    }

    // 驗證流程逾時後傳送一般文字時，會自動重置為 Idle 並提示逾時訊息
    [Fact]
    public async Task HandleAsync_WhenFlowTimesOut_ShouldResetToIdleAndReturnTimeoutMessage()
    {
        const string userId = "user-timeout-test";
        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "儲存");

        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.SaveFlow, context.State);

        // 模擬逾時 2 小時
        context.LastActiveAt = DateTime.UtcNow.AddHours(-2);
        await _stateManager.SaveCurrentAsync();

        var response = await HandleAsync(userId, "拉麵");

        Assert.Equal("先前的操作已逾時並已重置，請輸入「旅遊小幫手」開始。", response.Message);
        var resetContext = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.Idle, resetContext.State);
    }

    // 驗證流程逾時後輸入「旅遊小幫手」能直接重新喚醒，且保留原先的地區設定
    [Fact]
    public async Task HandleAsync_WhenFlowTimesOut_AndUserSendsWakeUp_ShouldWakeUpDirectly()
    {
        const string userId = "user-timeout-wakeup";
        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "切換地區");
        await HandleAsync(userId, "福岡");
        await HandleAsync(userId, "儲存");

        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.SaveFlow, context.State);
        Assert.Equal("福岡", context.CurrentDestination);

        // 模擬逾時 2 小時
        context.LastActiveAt = DateTime.UtcNow.AddHours(-2);
        await _stateManager.SaveCurrentAsync();

        var response = await HandleAsync(userId, "旅遊小幫手");

        Assert.Contains("目前地區：【福岡】", response.Message);
        var resetContext = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.MainMenu, resetContext.State);
        Assert.Equal("福岡", resetContext.CurrentDestination);
    }
}