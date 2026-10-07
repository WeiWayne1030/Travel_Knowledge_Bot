using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace OkinawaBot.Infrastructure.Line;

public class LineClient
{
    private readonly HttpClient _httpClient;
    private readonly LineBotOptions _options;
    private readonly ILogger<LineClient> _logger;

    public LineClient(HttpClient httpClient, IOptions<LineBotOptions> options, ILogger<LineClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        
        _httpClient.BaseAddress = new Uri("https://api.line.me/v2/bot/message/reply");
    }

    public async Task ReplyMessageAsync(string replyToken, string text)
    {
        if (string.IsNullOrEmpty(_options.ChannelAccessToken))
        {
            _logger.LogWarning("ChannelAccessToken is not set, skipping reply.");
            return;
        }

        var requestBody = new LineReplyRequest
        {
            ReplyToken = replyToken,
            Messages = new List<LineReplyMessage>
            {
                new LineReplyMessage { Text = text }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ChannelAccessToken);
        request.Content = content;

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send LINE reply. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
        }
    }
}

public class LineReplyRequest
{
    [JsonPropertyName("replyToken")]
    public string ReplyToken { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<LineReplyMessage> Messages { get; set; } = new();
}

public class LineReplyMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
