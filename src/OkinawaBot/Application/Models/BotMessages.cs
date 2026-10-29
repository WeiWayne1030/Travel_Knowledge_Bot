using OkinawaBot.Domain.Entities;

namespace OkinawaBot.Application.Models;

public static class BotMessages
{
    public const string WakeUpWord = "旅遊小幫手";

    public const string MainMenuPrompt = "請輸入 儲存、查詢、編輯、刪除、切換地區 或輸入「再見小幫手」退出。";

    public const string InvalidItemNumber = "請輸入有效的項目編號，或輸入「返回主選單」。";

    public const string ContinueOrReturnPrompt = "請輸入 繼續 或 返回主選單。";

    public const string SleepNotice = $"已退出小幫手。若要再次使用，請輸入「{WakeUpWord}」。";

    public const string OperationNotSupported = $"目前無法處理這個操作，請輸入「返回主選單」或「{WakeUpWord}」。";

    public static string FlowTimeoutNotice => $"先前的操作已逾時並已重置，請輸入「{WakeUpWord}」開始。";

    public static string ItemNumberOutOfRange(int max) =>
        $"請輸入 1 到 {max} 之間的項目編號，或輸入「返回主選單」。";

    public static string WithMainMenu(string message) =>
        $"{message}\n\n已回到主選單。\n{MainMenuPrompt}";

    public static string WithRetryHint(string message) =>
        $"{message}\n\n請重新輸入，或輸入「返回主選單」。";

    public static string NoItemsToProcess(string action) =>
        WithMainMenu($"目前沒有可以{action}的資料。");

    public static string ItemList(string action, IReadOnlyList<TravelItem> items) =>
        items.Count > 0
            ? $"\n目前可{action}的項目：\n" + string.Join("\n", items.Select((item, idx) => $"{idx + 1}. {item.Name} (#{item.Category})"))
            : "\n目前尚無任何項目。";

    public static readonly IReadOnlyList<string> MainMenuQuickReplies = new[]
    {
        "儲存", "查詢", "編輯", "刪除", "切換地區", "再見小幫手"
    };

    public static readonly IReadOnlyList<string> WakeUpQuickReplies = new[]
    {
        WakeUpWord
    };

    public static readonly IReadOnlyList<string> ReturnOrSleepQuickReplies = new[]
    {
        "返回主選單", "再見小幫手"
    };

    public static readonly IReadOnlyList<string> ContinueOrReturnQuickReplies = new[]
    {
        "繼續", "返回主選單"
    };
}
