using System.Collections.Concurrent;

namespace OkinawaBot.Application.State;

public class ConversationStateManager
{
    private readonly ConcurrentDictionary<string, ConversationContext> _contexts = new();

    public ConversationContext GetOrCreate(string userId)
    {
        return _contexts.GetOrAdd(userId, id => new ConversationContext
        {
            UserId = id,
            State = ConversationState.MainMenu
        });
    }

    public void SetState(string userId, ConversationState state)
    {
        var context = GetOrCreate(userId);
        context.State = state;
    }

    public void Reset(string userId)
    {
        var context = GetOrCreate(userId);
        context.State = ConversationState.MainMenu;
        context.CurrentItemId = null;
        context.PendingSave = null;
    }
}