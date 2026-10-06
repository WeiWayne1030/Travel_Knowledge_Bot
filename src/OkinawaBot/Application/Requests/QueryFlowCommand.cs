using MediatR;
using OkinawaBot.Application.Models;

namespace OkinawaBot.Application.Requests;

public class QueryFlowCommand : IRequest<BotResponse>
{
    public string UserId { get; }
    public string Message { get; }

    public QueryFlowCommand(string userId, string message)
    {
        UserId = userId;
        Message = message;
    }
}
