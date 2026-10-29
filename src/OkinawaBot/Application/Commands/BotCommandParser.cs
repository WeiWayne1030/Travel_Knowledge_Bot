namespace OkinawaBot.Application.Commands;

public class BotCommandParser
{
    public BotCommand Parse(string message)
    {
        return message.Trim().ToLowerInvariant() switch
        {
            "旅遊小幫手" => BotCommand.WakeUp,
            "儲存" => BotCommand.Save,
            "查詢" => BotCommand.Query,
            "編輯" => BotCommand.Edit,
            "刪除" => BotCommand.Delete,
            "返回主選單" => BotCommand.Return,
            "再見小幫手" => BotCommand.Sleep,
            "切換地區" or "切換目的地" => BotCommand.SwitchDestination,
            _ => BotCommand.Unknown
        };
    }
}