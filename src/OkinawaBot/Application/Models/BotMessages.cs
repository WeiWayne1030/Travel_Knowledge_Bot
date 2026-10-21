namespace OkinawaBot.Application.Models;

public static class BotMessages
{
    public const string MainMenuPrompt = "請輸入 儲存、查詢、編輯、刪除、切換地區 或輸入「再見小幫手」退出。";

    public const string InvalidItemNumber = "請輸入有效的項目編號，或輸入「返回主選單」。";

    public const string ContinueOrReturnPrompt = "請輸入 繼續 或 返回主選單。";

    public const string SleepNotice = "已退出小幫手。若要再次使用，請輸入「旅遊小幫手」。";

    public const string OperationNotSupported = "目前無法處理這個操作，請輸入「返回主選單」或「旅遊小幫手」。";

    public static string ItemNumberOutOfRange(int max) =>
        $"請輸入 1 到 {max} 之間的項目編號，或輸入「返回主選單」。";

    public static string WithMainMenu(string message) =>
        $"{message}\n\n已回到主選單。\n{MainMenuPrompt}";

    public static string WithRetryHint(string message) =>
        $"{message}\n\n請重新輸入，或輸入「返回主選單」。";

    public static string NoItemsToProcess(string action) =>
        WithMainMenu($"目前沒有可以{action}的資料。");
}
