using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace OkinawaBot.Application.State;

public class ConversationStateManager
{
    private readonly IDistributedCache _cache;
    private ConversationContext? _currentContext;

    public ConversationStateManager(IDistributedCache cache)
    {
        _cache = cache;
    }

    public ConversationContext GetOrCreate(string userId)
    {
        if (_currentContext != null && _currentContext.UserId == userId)
        {
            return _currentContext;
        }

        var cachedData = _cache.GetString(userId);
        if (!string.IsNullOrEmpty(cachedData))
        {
            _currentContext = JsonSerializer.Deserialize<ConversationContext>(cachedData);
        }
        else
        {
            _currentContext = null;
        }

        if (_currentContext == null)
        {
            _currentContext = new ConversationContext
            {
                UserId = userId,
                State = ConversationState.MainMenu
            };
        }

        return _currentContext;
    }

    public async Task SaveCurrentAsync()
    {
        if (_currentContext != null)
        {
            //設定Session有效時間為一小時
            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(1));
                
            var json = JsonSerializer.Serialize(_currentContext);
            await _cache.SetStringAsync(_currentContext.UserId, json, options);
        }
    }

    //更新狀態就回寫redis
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
        context.PendingEdit = null;
    }
}