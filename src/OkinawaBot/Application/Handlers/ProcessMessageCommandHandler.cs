using MediatR;
using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Requests;
using OkinawaBot.Application.State;

namespace OkinawaBot.Application.Handlers;

public class ProcessMessageCommandHandler : IRequestHandler<ProcessMessageCommand, BotResponse>
{
    private readonly ConversationStateManager _stateManager;
    private readonly BotCommandParser _commandParser;
    private readonly IMediator _mediator;

    public ProcessMessageCommandHandler(
        ConversationStateManager stateManager,
        BotCommandParser commandParser,
        IMediator mediator)
    {
        _stateManager = stateManager;
        _commandParser = commandParser;
        _mediator = mediator;
    }

    public async Task<BotResponse> Handle(ProcessMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var message = request.Message;

        var context = _stateManager.GetOrCreate(userId);

        BotResponse response;

        if (context.State == ConversationState.MainMenu)
        {
            response = HandleMainMenu(userId, message);
        }
        else if (context.State == ConversationState.SaveFlow ||
            context.State == ConversationState.SaveDuplicateConfirmation)
        {
            response = await _mediator.Send(new SaveFlowCommand(userId, message), cancellationToken);
        }
        else if (context.State == ConversationState.QueryFlow)
        {
            response = await _mediator.Send(new QueryFlowCommand(userId, message), cancellationToken);
        }
        else if (context.State == ConversationState.EditItemSelection ||
            context.State == ConversationState.EditDataInput ||
            context.State == ConversationState.EditDuplicateConfirmation)
        {
            response = await _mediator.Send(new EditFlowCommand(userId, message), cancellationToken);
        }
        else if (context.State == ConversationState.DeleteFlow ||
            context.State == ConversationState.DeleteConfirmation)
        {
            response = await _mediator.Send(new DeleteFlowCommand(userId, message), cancellationToken);
        }
        else
        {
            response = new BotResponse
            {
                Message = "目前無法處理這個操作。"
            };
        }

        await _stateManager.SaveCurrentAsync();
        return response;
    }

    private BotResponse HandleMainMenu(
        string userId,
        string message)
    {
        var command = _commandParser.Parse(message);

        switch (command)
        {
            case BotCommand.Save:
                _stateManager.SetState(userId, ConversationState.SaveFlow);
                return new BotResponse
                {
                    Message =
                        "請輸入旅遊資訊：\n" +
                        "URL\n" +
                        "#Category\n" +
                        "Name"
                };

            case BotCommand.Query:
                _stateManager.SetState(userId, ConversationState.QueryFlow);
                return new BotResponse
                {
                    Message = "請輸入要查詢的 Category。"
                };

            case BotCommand.Edit:
                _stateManager.SetState(userId, ConversationState.EditItemSelection);
                return new BotResponse
                {
                    Message = "請輸入要編輯的項目編號。"
                };

            case BotCommand.Delete:
                _stateManager.SetState(userId, ConversationState.DeleteFlow);
                return new BotResponse
                {
                    Message = "請輸入要刪除的項目編號。"
                };

            default:
                return new BotResponse
                {
                    Message = "請輸入 Save、Query、Edit 或 Delete。"
                };
        }
    }
}
