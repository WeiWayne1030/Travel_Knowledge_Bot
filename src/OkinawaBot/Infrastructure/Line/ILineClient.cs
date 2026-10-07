using System.Threading.Tasks;

//建立interface是為了測試可以透過DI更換實作的用意
namespace OkinawaBot.Infrastructure.Line;

public interface ILineClient
{
    Task ReplyMessageAsync(string replyToken, string text);
}
