namespace OkinawaBot.Application.Models;

public class SaveResult
{
    public SaveResultStatus Status { get; set; }

    public int? ItemId { get; set; }

    public string? Message { get; set; }
}
