using MediatR;
using OkinawaBot.Application.Models;

namespace OkinawaBot.Application.Requests;

public class DeleteFlowCommand : IRequest<BotResponse>
{
    public string UserId { get; }
    public string Message { get; }

    public DeleteFlowCommand(string userId, string message)
    {
        UserId = userId;
        Message = message;
    }
}
