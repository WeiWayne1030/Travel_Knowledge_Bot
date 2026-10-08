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
    private readonly OkinawaBot.Application.Services.QueryService _queryService;

    public ProcessMessageCommandHandler(
        ConversationStateManager stateManager,
        BotCommandParser commandParser,
        IMediator mediator,
        OkinawaBot.Application.Services.QueryService queryService)
    {
        _stateManager = stateManager;
        _commandParser = commandParser;
        _mediator = mediator;
        _queryService = queryService;
    }

    public async Task<BotResponse> Handle(ProcessMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var message = request.Message;

        var context = _stateManager.GetOrCreate(userId);

        BotResponse response;

        if (context.State == ConversationState.Idle)
        {
            if (message.Trim() == "旅遊小幫手")
            {
                _stateManager.SetState(userId, ConversationState.MainMenu);
                response = new BotResponse
                {
                    Message = "您好！我是旅遊小幫手。\n請輸入 儲存、查詢、編輯、刪除或輸入「再見小幫手」退出。"
                };
            }
            else
            {
                response = new BotResponse
                {
                    Message = string.Empty
                };
            }
        }
        else if (context.State == ConversationState.MainMenu)
        {
            response = await HandleMainMenuAsync(userId, message);
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

    private async Task<BotResponse> HandleMainMenuAsync(
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
                        "#類別\n" +
                        "名稱"
                };

            case BotCommand.Query:
                _stateManager.SetState(userId, ConversationState.QueryFlow);
                var categories = await _queryService.GetAvailableCategoriesAsync();
                var categoriesMsg = categories.Any() 
                    ? "\n目前有的類別：\n" + string.Join("\n", categories)
                    : "\n目前還沒有任何類別。";

                return new BotResponse
                {
                    Message = $"""請輸入要查詢的類別或"返回選單"。{categoriesMsg}"""
                };

            case BotCommand.Edit:
                _stateManager.SetState(userId, ConversationState.EditItemSelection);
                return new BotResponse
                {
                    Message = """請輸入要編輯的項目編號或"返回選單"。"""
                };

            case BotCommand.Delete:
                _stateManager.SetState(userId, ConversationState.DeleteFlow);
                return new BotResponse
                {
                    Message = """請輸入要刪除的項目編號或"返回選單"。"""
                };

            case BotCommand.Sleep:
                _stateManager.Reset(userId);
                return new BotResponse
                {
                    Message = "已退出小幫手。"
                };

            default:
                return new BotResponse
                {
                    Message = "請輸入 儲存、查詢、編輯、刪除或輸入「再見小幫手」退出。"
                };
        }
    }
}
