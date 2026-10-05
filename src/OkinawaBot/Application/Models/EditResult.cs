using OkinawaBot.Domain.Entities;

namespace OkinawaBot.Application.Models;

public enum EditResultStatus
{
    Success,
    NotFound,
    Duplicate
}

public class EditResult
{
    public EditResultStatus Status { get; init; }

    public TravelItem? Item { get; init; }

    public static EditResult Success(
        TravelItem item)
    {
        return new EditResult
        {
            Status = EditResultStatus.Success,
            Item = item
        };
    }

    public static EditResult NotFound()
    {
        return new EditResult
        {
            Status = EditResultStatus.NotFound
        };
    }

    public static EditResult Duplicate()
    {
        return new EditResult
        {
            Status = EditResultStatus.Duplicate
        };
    }
}
