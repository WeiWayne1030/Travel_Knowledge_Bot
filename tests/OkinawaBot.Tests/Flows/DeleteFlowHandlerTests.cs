using System.Threading;
using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Flows;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;
using OkinawaBot.Domain.Entities;
using OkinawaBot.Tests.Fakes;

namespace OkinawaBot.Tests.Flows;

public class DeleteFlowHandlerTests
{
    //沒資料刪
    [Fact]
    public async Task HandleAsync_WhenNoItems_ShouldReturnToMainMenu()
    {
        // Arrange
        var stateManager =
            new ConversationStateManager(new FakeDistributedCache());

        var repository =
            new FakeTravelItemRepository();

        var deleteService =
            new DeleteService(repository);

        var handler =
            new DeleteFlowHandler(
                stateManager,
                deleteService);

        stateManager.SetState(
            "test-user",
            ConversationState.DeleteFlow);

        // Act
        var response =
            await handler.Handle(new OkinawaBot.Application.Requests.DeleteFlowCommand("test-user", "1"), CancellationToken.None);

        // Assert
        var context =
            stateManager.GetOrCreate("test-user");

        Assert.Equal(
            ConversationState.MainMenu,
            context.State);

        Assert.Contains(
            "目前沒有可以刪除的資料",
            response.Message);
    }

    //有資料也有編號
    [Fact]
    public async Task HandleAsync_WhenValidItemNumber_ShouldEnterConfirmation()
    {
        // Arrange
        var stateManager =
            new ConversationStateManager(new FakeDistributedCache());

        var repository =
            new FakeTravelItemRepository();

        await repository.CreateAsync(new TravelItem
        {
            Id = 1,
            Name = "美麗海水族館",
            Url = "https://example.com",
            Category = "Attraction"
        });

        var deleteService =
            new DeleteService(repository);

        var handler =
            new DeleteFlowHandler(
                stateManager,
                deleteService);

        stateManager.SetState(
            "test-user",
            ConversationState.DeleteFlow);

        // Act
        var response =
            await handler.Handle(new OkinawaBot.Application.Requests.DeleteFlowCommand("test-user", "1"), CancellationToken.None);

        // Assert
        var context =
            stateManager.GetOrCreate("test-user");

        Assert.Equal(
            ConversationState.DeleteConfirmation,
            context.State);

        Assert.Equal(
            1,
            context.CurrentItemId);

        Assert.Contains(
            "美麗海水族館",
            response.Message);

        Assert.Contains(
            "繼續",
            response.Message);

        Assert.Contains(
            "返回主選單",
            response.Message);
    }

    //輸入非數字
    [Fact]
    public async Task HandleAsync_WhenItemNumberIsInvalid_ShouldStayInDeleteFlow()
    {
        // Arrange
        var stateManager =
            new ConversationStateManager(new FakeDistributedCache());

        var repository =
            new FakeTravelItemRepository();

        await repository.CreateAsync(new TravelItem
        {
            Id = 1,
            Name = "美麗海水族館",
            Url = "https://example.com",
            Category = "Attraction"
        });

        var deleteService =
            new DeleteService(repository);

        var handler =
            new DeleteFlowHandler(
                stateManager,
                deleteService);

        stateManager.SetState(
            "test-user",
            ConversationState.DeleteFlow);

        // Act
        var response =
            await handler.Handle(new OkinawaBot.Application.Requests.DeleteFlowCommand("test-user", "abc"), CancellationToken.None);

        // Assert
        var context =
            stateManager.GetOrCreate("test-user");

        Assert.Equal(
            ConversationState.DeleteFlow,
            context.State);

        Assert.Contains(
            "請輸入有效的項目編號",
            response.Message);
    }

    //輸入超出範圍
    [Fact]
    public async Task HandleAsync_WhenItemNumberOutOfRange_ShouldStayInDeleteFlow()
    {
        // Arrange
        var stateManager =
            new ConversationStateManager(new FakeDistributedCache());

        var repository =
            new FakeTravelItemRepository();

        await repository.CreateAsync(new TravelItem
        {
            Id = 1,
            Name = "美麗海水族館",
            Url = "https://example.com",
            Category = "Attraction"
        });

        var deleteService =
            new DeleteService(repository);

        var handler =
            new DeleteFlowHandler(
                stateManager,
                deleteService);

        stateManager.SetState(
            "test-user",
            ConversationState.DeleteFlow);

        // Act
        var response =
            await handler.Handle(new OkinawaBot.Application.Requests.DeleteFlowCommand("test-user", "99"), CancellationToken.None);

        // Assert
        var context =
            stateManager.GetOrCreate("test-user");

        Assert.Equal(
            ConversationState.DeleteFlow,
            context.State);

        Assert.Contains(
            "1 到 1",
            response.Message);
    }

    //confirmation => continue
    [Fact]
    public async Task HandleAsync_WhenConfirmationContinues_ShouldDeleteItem()
    {
        // Arrange
        var stateManager =
            new ConversationStateManager(new FakeDistributedCache());

        var repository =
            new FakeTravelItemRepository();

        await repository.CreateAsync(new TravelItem
        {
            Id = 1,
            Name = "美麗海水族館",
            Url = "https://example.com",
            Category = "Attraction"
        });

        var deleteService =
            new DeleteService(repository);

        var handler =
            new DeleteFlowHandler(
                stateManager,
                deleteService);

        var context =
            stateManager.GetOrCreate("test-user");

        context.State =
            ConversationState.DeleteConfirmation;

        context.CurrentItemId = 1;

        // Act
        var response =
            await handler.Handle(new OkinawaBot.Application.Requests.DeleteFlowCommand("test-user", "繼續"), CancellationToken.None);

        // Assert
        Assert.Equal(
            ConversationState.MainMenu,
            context.State);

        Assert.Null(
            context.CurrentItemId);

        var deletedItem =
            await repository.FindByIdAsync(1);

        Assert.Null(deletedItem);

        Assert.Contains(
            "刪除成功",
            response.Message);
    }

    //Confirmation 輸入錯誤
    [Fact]
    public async Task HandleAsync_WhenConfirmationIsInvalid_ShouldStayInConfirmation()
    {
        // Arrange
        var stateManager =
            new ConversationStateManager(new FakeDistributedCache());

        var repository =
            new FakeTravelItemRepository();

        await repository.CreateAsync(new TravelItem
        {
            Id = 1,
            Name = "美麗海水族館",
            Url = "https://example.com",
            Category = "Attraction"
        });

        var deleteService =
            new DeleteService(repository);

        var handler =
            new DeleteFlowHandler(
                stateManager,
                deleteService);

        var context =
            stateManager.GetOrCreate("test-user");

        context.State =
            ConversationState.DeleteConfirmation;

        context.CurrentItemId = 1;

        // Act
        var response =
            await handler.Handle(new OkinawaBot.Application.Requests.DeleteFlowCommand("test-user", "hello"), CancellationToken.None);

        // Assert
        Assert.Equal(
            ConversationState.DeleteConfirmation,
            context.State);

        var item =
            await repository.FindByIdAsync(1);

        Assert.NotNull(item);

        Assert.Contains(
            "繼續 或 返回主選單",
            response.Message);
    }

    // 驗證刪除時僅能選取當前地區的項目
    [Fact]
    public async Task DeleteFlow_ShouldOnlyShowItemsFromCurrentDestination()
    {
        var stateManager = new ConversationStateManager(new FakeDistributedCache());
        var repository = new FakeTravelItemRepository();

        await repository.CreateAsync(new TravelItem
        {
            Destination = "首爾",
            Name = "景福宮",
            Url = "https://example.com/seoul",
            Category = "Attraction"
        });

        await repository.CreateAsync(new TravelItem
        {
            Destination = "沖繩",
            Name = "美麗海水族館",
            Url = "https://example.com/okinawa",
            Category = "Attraction"
        });

        var deleteService = new DeleteService(repository);
        var handler = new DeleteFlowHandler(stateManager, deleteService);

        var context = stateManager.GetOrCreate("test-dest-delete");
        context.CurrentDestination = "首爾";
        context.State = ConversationState.DeleteFlow;

        // 在首爾只有 1 筆
        var response = await handler.Handle(new OkinawaBot.Application.Requests.DeleteFlowCommand("test-dest-delete", "1"), CancellationToken.None);

        Assert.Equal(ConversationState.DeleteConfirmation, context.State);
        Assert.Contains("景福宮", response.Message);
    }
}
