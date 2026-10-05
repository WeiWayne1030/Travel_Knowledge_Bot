using System.Text.RegularExpressions;

namespace OkinawaBot.Application.Input;

public class SaveInputParser
{
    public SaveInputParseResult Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return SaveInputParseResult.Failure(
                "輸入內容不可以是空的。");
        }

        var categoryMatch =
            Regex.Match(
                input,
                @"#(?<category>\S+)");

        if (!categoryMatch.Success)
        {
            return SaveInputParseResult.Failure(
                "缺少分類，請使用 #分類 格式，例如 #Attraction。");
        }

        var category =
            categoryMatch.Groups["category"].Value;

        var beforeCategory =
            input[..categoryMatch.Index].Trim();

        var afterCategory =
            input[(categoryMatch.Index + categoryMatch.Length)..]
                .Trim();

        if (string.IsNullOrWhiteSpace(beforeCategory))
        {
            return SaveInputParseResult.Failure(
                "缺少 URL，請將 URL 放在 #分類 之前。");
        }

        if (string.IsNullOrWhiteSpace(afterCategory))
        {
            return SaveInputParseResult.Failure(
                "缺少名稱，請將名稱放在 #分類 之後。");
        }

        return SaveInputParseResult.Success(
            beforeCategory,
            category,
            afterCategory);
    }
}