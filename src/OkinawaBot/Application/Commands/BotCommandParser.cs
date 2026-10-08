namespace OkinawaBot.Application.Commands;

public class BotCommandParser
{
    public BotCommand Parse(string message)
    {
        return message.Trim().ToLowerInvariant() switch
        {
            "save" => BotCommand.Save,
            "query" => BotCommand.Query,
            "edit" => BotCommand.Edit,
            "delete" => BotCommand.Delete,
            "return" => BotCommand.Return,
            "再見小幫手" => BotCommand.Sleep,
            _ => BotCommand.Unknown
        };
    }
}