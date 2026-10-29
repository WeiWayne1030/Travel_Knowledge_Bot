using System.Collections.Concurrent;
using System.Threading.Tasks;
using OkinawaBot.Infrastructure.Line;

namespace OkinawaBot.Tests.Fakes;

public class FakeLineClient : ILineClient
{
    public ConcurrentDictionary<string, string> SentMessages { get; } = new();
    public ConcurrentDictionary<string, IReadOnlyList<string>?> SentQuickReplies { get; } = new();

    public Task ReplyMessageAsync(string replyToken, string message, IReadOnlyList<string>? quickReplies = null)
    {
        if (!string.IsNullOrEmpty(replyToken))
        {
            SentMessages[replyToken] = message;
            SentQuickReplies[replyToken] = quickReplies;
        }
        return Task.CompletedTask;
    }
}
