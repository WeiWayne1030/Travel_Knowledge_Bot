using System.Text.RegularExpressions;

namespace OkinawaBot.Application.Input;

public class EditInputParser
{
    private static readonly Regex ExplicitUrlRegex =
        new(@"(?:網址|url)\s*[:：]\s*(?<url>\S+)", RegexOptions.IgnoreCase);

    private static readonly Regex DirectUrlRegex =
        new(@"(?<url>[a-zA-Z][a-zA-Z0-9+.-]*://\S+)", RegexOptions.IgnoreCase);

    private static readonly Regex ExplicitCategoryRegex =
        new(@"(?:類別|分類|category)\s*[:：]\s*#?(?<cat>[^\s#]+)", RegexOptions.IgnoreCase);

    private static readonly Regex HashCategoryRegex =
        new(@"#(?<cat>[^\s#]+)");

    private static readonly Regex ExplicitNameRegex =
        new(@"(?:名稱|name)\s*[:：]\s*(?<name>.+)", RegexOptions.IgnoreCase);

    public EditInputParseResult Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return EditInputParseResult.Failure("輸入內容不可以是空的。");
        }

        var trimmed = input.Trim();

        if (trimmed == "#")
        {
            return EditInputParseResult.Failure("缺少分類名稱，請使用 #分類 格式，例如 #Attraction。");
        }

        string? url = null;
        string? category = null;
        string? name = null;

        var workingText = trimmed;

        // 1. 解析 URL (先看是否有前綴，再看直接 URL)
        var explicitUrlMatch = ExplicitUrlRegex.Match(workingText);
        if (explicitUrlMatch.Success)
        {
            url = explicitUrlMatch.Groups["url"].Value;
            workingText = workingText.Remove(explicitUrlMatch.Index, explicitUrlMatch.Length);
        }
        else
        {
            var directUrlMatch = DirectUrlRegex.Match(workingText);
            if (directUrlMatch.Success)
            {
                url = directUrlMatch.Groups["url"].Value;
                workingText = workingText.Remove(directUrlMatch.Index, directUrlMatch.Length);
            }
        }

        // 驗證 URL
        if (url is not null)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                return EditInputParseResult.Failure("URL 格式不正確，必須以 http:// 或 https:// 開頭。");
            }
        }

        // 2. 解析分類 (先看是否有「類別:」前綴，再看 #分類)
        var explicitCatMatch = ExplicitCategoryRegex.Match(workingText);
        if (explicitCatMatch.Success)
        {
            category = explicitCatMatch.Groups["cat"].Value;
            workingText = workingText.Remove(explicitCatMatch.Index, explicitCatMatch.Length);
        }
        else
        {
            var hashCatMatch = HashCategoryRegex.Match(workingText);
            if (hashCatMatch.Success)
            {
                category = hashCatMatch.Groups["cat"].Value;
                workingText = workingText.Remove(hashCatMatch.Index, hashCatMatch.Length);
            }
        }

        // 3. 解析名稱 (先看是否有「名稱:」前綴，否則取剩餘字串)
        var explicitNameMatch = ExplicitNameRegex.Match(workingText);
        if (explicitNameMatch.Success)
        {
            name = explicitNameMatch.Groups["name"].Value.Trim();
            workingText = workingText.Remove(explicitNameMatch.Index, explicitNameMatch.Length);
        }
        else
        {
            var remaining = workingText.Trim();
            if (remaining.StartsWith('#'))
            {
                return EditInputParseResult.Failure("缺少分類名稱，請使用 #分類 格式，例如 #Attraction。");
            }

            if (!string.IsNullOrWhiteSpace(remaining))
            {
                name = remaining;
            }
        }

        if (string.IsNullOrWhiteSpace(url) &&
            string.IsNullOrWhiteSpace(category) &&
            string.IsNullOrWhiteSpace(name))
        {
            return EditInputParseResult.Failure("請提供要修改的資料（可包含 URL、#類別 或 名稱）。");
        }

        return EditInputParseResult.Success(url, category, name);
    }
}
