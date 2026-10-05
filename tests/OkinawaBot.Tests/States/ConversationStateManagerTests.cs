using OkinawaBot.Application.State;

namespace OkinawaBot.Tests.State;

public class ConversationStateManagerTests
{
    [Fact]
    public void GetOrCreate_ShouldReturnMainMenu_ForNewUser()
    {
        // Arrange
        var manager = new ConversationStateManager();

        // Act
        var context = manager.GetOrCreate("user-001");

        // Assert
        Assert.Equal(
            ConversationState.MainMenu,
            context.State);
    }

    [Fact]
    public void SetState_ShouldChangeUserState()
    {
        // Arrange
        var manager = new ConversationStateManager();

        // Act
        manager.SetState(
            "user-001",
            ConversationState.SaveFlow);

        var context = manager.GetOrCreate("user-001");

        // Assert
        Assert.Equal(
            ConversationState.SaveFlow,
            context.State);
    }

    [Fact]
    public void Reset_ShouldReturnUserToMainMenu()
    {
        // Arrange
        var manager = new ConversationStateManager();

        manager.SetState(
            "user-001",
            ConversationState.SaveFlow);

        // Act
        manager.Reset("user-001");

        var context = manager.GetOrCreate("user-001");

        // Assert
        Assert.Equal(
            ConversationState.MainMenu,
            context.State);

        Assert.Null(context.CurrentItemId);
        Assert.Null(context.PendingSave);
    }

    [Fact]
    public void Users_ShouldHaveIndependentStates()
    {
        // Arrange
        var manager = new ConversationStateManager();

        // Act
        manager.SetState(
            "user-A",
            ConversationState.SaveFlow);

        manager.SetState(
            "user-B",
            ConversationState.QueryFlow);

        var userA = manager.GetOrCreate("user-A");
        var userB = manager.GetOrCreate("user-B");

        // Assert
        Assert.Equal(
            ConversationState.SaveFlow,
            userA.State);

        Assert.Equal(
            ConversationState.QueryFlow,
            userB.State);
    }
}
