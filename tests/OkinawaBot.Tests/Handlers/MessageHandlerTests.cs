using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Flows;
using OkinawaBot.Application.Handlers;
using OkinawaBot.Application.Input;
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
            _mediator);
    }

    private Task HandleAsync(string userId, string message)
    {
        return _handler.Handle(new ProcessMessageCommand(userId, message), CancellationToken.None);
    }

    // 驗證收到 Save 指令時，會正確切換至 SaveFlow 狀態
    [Fact]
    public async Task HandleAsync_ShouldEnterSaveFlow_WhenUserSendsSave()
    {
        const string userId = "user-001";
        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "Save");

        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.SaveFlow, context.State);
    }

    // 驗證收到 Query 指令時，會正確切換至 QueryFlow 狀態
    [Fact]
    public async Task HandleAsync_ShouldEnterQueryFlow_WhenUserSendsQuery()
    {
        const string userId = "user-001";
        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "Query");

        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.QueryFlow, context.State);
    }

    // 驗證在 SaveFlow 下輸入有效格式時，能順利儲存旅遊資訊
    [Fact]
    public async Task HandleAsync_ShouldSaveItem_WhenSaveInputIsValid()
    {
        const string userId = "user-001";

        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "Save");
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
        await HandleAsync(userId, "Save");
        await HandleAsync(userId, "https://example.com 美麗海水族館");

        Assert.Empty(_repository.Items);
    }

    // 驗證名稱已存在時，不會建立重複的資料
    [Fact]
    public async Task HandleAsync_ShouldNotCreateDuplicate_WhenNameAlreadyExists()
    {
        const string userId = "user-001";

        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "Save");
        await HandleAsync(userId, "https://example.com #ATTRACTION 美麗海水族館");

        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "Save");
        await HandleAsync(userId, "https://example.com/2 #ATTRACTION 美麗海水族館");

        Assert.Single(_repository.Items);
    }

    // 驗證輸入 Return 時，能正確返回 MainMenu
    [Fact]
    public async Task HandleAsync_ShouldReturnToMainMenu_WhenUserEntersReturn()
    {
        const string userId = "user-001";

        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "Save");
        await HandleAsync(userId, "return");

        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.Idle, context.State);
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

        await HandleAsync(userId, "旅遊小幫手");
        await HandleAsync(userId, "Edit");

        var context = _stateManager.GetOrCreate(userId);
        Assert.Equal(ConversationState.EditItemSelection, context.State);
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
        await HandleAsync("test-user", "旅遊小幫手");
        await HandleAsync("test-user", "Delete");

        var context = _stateManager.GetOrCreate("test-user");
        Assert.Equal(ConversationState.DeleteFlow, context.State);
    }

    [Fact]
    public async Task HandleAsync_WhenInDeleteFlowAndReturn_ShouldGoBackToMainMenu()
    {
        await HandleAsync("test-user", "旅遊小幫手");
        await HandleAsync("test-user", "Delete");
        await HandleAsync("test-user", "Return");

        var context = _stateManager.GetOrCreate("test-user");
        Assert.Equal(ConversationState.Idle, context.State);
    }
}