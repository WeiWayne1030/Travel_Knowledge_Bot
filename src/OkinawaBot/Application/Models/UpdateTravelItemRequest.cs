namespace OkinawaBot.Application.Models;

public class UpdateTravelItemRequest
{
    public string? Destination { get; init; }

    public string? Url { get; init; }

    public string? Category { get; init; }

    public string? Name { get; init; }
}
