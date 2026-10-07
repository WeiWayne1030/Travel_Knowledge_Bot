namespace OkinawaBot.Infrastructure.Line;

//設定secrect & token
public class LineBotOptions
{
    public const string SectionName = "LineBot";

    public string ChannelSecret { get; set; } = string.Empty;
    public string ChannelAccessToken { get; set; } = string.Empty;
}
