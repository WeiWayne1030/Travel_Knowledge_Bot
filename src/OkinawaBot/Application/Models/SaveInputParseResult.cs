namespace OkinawaBot.Application.Input;

//文字解析的結果
public class SaveInputParseResult
{
    public bool IsSuccess { get; init; }

    public string? ErrorMessage { get; init; }

    public string? Url { get; init; }

    public string? Category { get; init; }

    public string? Name { get; init; }

    public static SaveInputParseResult Success(string url, string category, string name)
    {
        return new SaveInputParseResult
        {
            IsSuccess = true,
            Url = url,
            Category = category,
            Name = name
        };
    }

    public static SaveInputParseResult Failure(string errorMessage)
    {
        return new SaveInputParseResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}