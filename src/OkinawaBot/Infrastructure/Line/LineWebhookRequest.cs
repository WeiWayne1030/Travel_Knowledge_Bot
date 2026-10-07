using System.Text.Json.Serialization;

namespace OkinawaBot.Infrastructure.Line;

public class LineWebhookRequest
{
    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;

    [JsonPropertyName("events")]
    public List<LineWebhookEvent> Events { get; set; } = new();
}

public class LineWebhookEvent
{
    [JsonPropertyName("replyToken")]
    public string ReplyToken { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("mode")]
    public string Mode { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("source")]
    public LineSource Source { get; set; } = new();

    [JsonPropertyName("message")]
    public LineMessage Message { get; set; } = new();
}

public class LineSource
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
}

public class LineMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
