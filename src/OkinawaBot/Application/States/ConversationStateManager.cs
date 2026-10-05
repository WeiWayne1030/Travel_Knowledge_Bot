namespace OkinawaBot.Application.State;

public class ConversationStateManager
{
    private readonly Dictionary<string, ConversationContext>
        _contexts = [];

    public ConversationContext GetOrCreate(
        string userId)
    {
        if (!_contexts.TryGetValue(
            userId,
            out var context))
        {
            context = new ConversationContext
            {
                UserId = userId,
                State = ConversationState.MainMenu
            };

            _contexts[userId] = context;
        }

        return context;
    }

    public void SetState(
        string userId,
        ConversationState state)
    {
        var context = GetOrCreate(userId);

        context.State = state;
    }

    public void Reset(string userId)
    {
        var context = GetOrCreate(userId);

        context.State =
            ConversationState.MainMenu;

        context.CurrentItemId = null;

        context.PendingSave = null;

        context.PendingEdit = null;
    }
}