namespace OkinawaBot.Application.Input;

public class EditInputParseResult
{
    public bool IsSuccess { get; init; }

    public string? ErrorMessage { get; init; }

    public string? Url { get; init; }

    public string? Category { get; init; }

    public string? Name { get; init; }

    public bool HasAnyChanges =>
        !string.IsNullOrWhiteSpace(Url) ||
        !string.IsNullOrWhiteSpace(Category) ||
        !string.IsNullOrWhiteSpace(Name);

    public static EditInputParseResult Success(string? url, string? category, string? name)
    {
        return new EditInputParseResult
        {
            IsSuccess = true,
            Url = url,
            Category = category,
            Name = name
        };
    }

    public static EditInputParseResult Failure(string errorMessage)
    {
        return new EditInputParseResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
