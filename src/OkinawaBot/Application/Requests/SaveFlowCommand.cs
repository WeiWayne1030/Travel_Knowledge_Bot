using MediatR;
using OkinawaBot.Application.Models;

namespace OkinawaBot.Application.Requests;

public class SaveFlowCommand : IRequest<BotResponse>
{
    public string UserId { get; }
    public string Message { get; }

    public SaveFlowCommand(string userId, string message)
    {
        UserId = userId;
        Message = message;
    }
}
