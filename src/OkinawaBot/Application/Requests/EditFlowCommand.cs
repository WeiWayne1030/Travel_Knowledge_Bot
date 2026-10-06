using MediatR;
using OkinawaBot.Application.Models;

namespace OkinawaBot.Application.Requests;

public class EditFlowCommand : IRequest<BotResponse>
{
    public string UserId { get; }
    public string Message { get; }

    public EditFlowCommand(string userId, string message)
    {
        UserId = userId;
        Message = message;
    }
}
