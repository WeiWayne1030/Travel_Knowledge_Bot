namespace OkinawaBot.Application.Models;

public class UpdateTravelItemRequest
{
    public string Url { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}
