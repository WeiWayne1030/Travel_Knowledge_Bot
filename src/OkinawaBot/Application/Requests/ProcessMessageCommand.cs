using MediatR;
using OkinawaBot.Application.Models;

namespace OkinawaBot.Application.Requests;

public class ProcessMessageCommand : IRequest<BotResponse>
{
    public string UserId { get; }
    public string Message { get; }

    public ProcessMessageCommand(string userId, string message)
    {
        UserId = userId;
        Message = message;
    }
}
