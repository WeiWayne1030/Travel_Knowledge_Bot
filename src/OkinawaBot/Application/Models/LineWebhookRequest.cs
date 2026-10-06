namespace OkinawaBot.Models;

public class LineWebhookRequest
{
    public List<LineEvent> Events { get; set; } = [];
}

public class LineEvent
{
    public string Type { get; set; } = string.Empty;

    public LineSource Source { get; set; } = new();

    public LineMessage Message { get; set; } = new();
}

public class LineSource
{
    public string UserId { get; set; } = string.Empty;
}

public class LineMessage
{
    public string Type { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
}