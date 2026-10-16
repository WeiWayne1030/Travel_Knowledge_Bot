using MediatR;
using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Requests;
using OkinawaBot.Application.State;

namespace OkinawaBot.Application.Handlers;

public class ProcessMessageCommandHandler : IRequestHandler<ProcessMessageCommand, BotResponse>
{
    private static string GetWakeUpNotice(string destination) =>
        $"您好！我是旅遊小幫手。\n目前地區：【{destination}】 📍\n請輸入 儲存、查詢、編輯、刪除、切換地區 或輸入「再見小幫手」退出。";

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

        // 檢查暫態流程是否逾時
        if (_stateManager.IsFlowExpired(context))
        {
            _stateManager.Reset(userId);

            if (message.Trim() == "旅遊小幫手")
            {
                _stateManager.SetState(userId, ConversationState.MainMenu);
                response = new BotResponse
                {
                    Message = GetWakeUpNotice(context.CurrentDestination)
                };
            }
            else
            {
                response = new BotResponse
                {
                    Message = "先前的操作已逾時並已重置，請輸入「旅遊小幫手」開始。"
                };
            }

            await _stateManager.SaveCurrentAsync();
            return response;
        }

        context.LastActiveAt = DateTime.UtcNow;

        if (context.State == ConversationState.Idle)
        {
            if (message.Trim() == "旅遊小幫手")
            {
                _stateManager.SetState(userId, ConversationState.MainMenu);
                response = new BotResponse
                {
                    Message = GetWakeUpNotice(context.CurrentDestination)
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
        else if (_commandParser.Parse(message) == BotCommand.Sleep)
        {
            _stateManager.Reset(userId);
            response = new BotResponse
            {
                Message = "已退出小幫手。"
            };
        }
        else if (_commandParser.Parse(message) == BotCommand.Return)
        {
            _stateManager.Reset(userId);
            _stateManager.SetState(userId, ConversationState.MainMenu);
            response = new BotResponse
            {
                Message = GetWakeUpNotice(context.CurrentDestination)
            };
        }
        else if (context.State == ConversationState.MainMenu)
        {
            response = await HandleMainMenuAsync(userId, message, context);
        }
        else if (context.State == ConversationState.SwitchDestination)
        {
            response = await HandleSwitchDestinationAsync(userId, message, context);
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
        string message,
        ConversationContext context)
    {
        var command = _commandParser.Parse(message);

        switch (command)
        {
            case BotCommand.Save:
                _stateManager.SetState(userId, ConversationState.SaveFlow);
                return new BotResponse
                {
                    Message =
                        $"【{context.CurrentDestination}】請輸入旅遊資訊：\n" +
                        "URL\n" +
                        "#類別\n" +
                        "名稱"
                };

            case BotCommand.Query:
                _stateManager.SetState(userId, ConversationState.QueryFlow);
                var categories = await _queryService.GetAvailableCategoriesAsync(context.CurrentDestination);
                var categoriesMsg = categories.Any() 
                    ? $"\n【{context.CurrentDestination}】目前有的類別：\n" + string.Join("\n", categories)
                    : $"\n【{context.CurrentDestination}】目前還沒有任何類別。";

                return new BotResponse
                {
                    Message = $"""請輸入要查詢的類別或"返回主選單"。{categoriesMsg}"""
                };

            case BotCommand.Edit:
                _stateManager.SetState(userId, ConversationState.EditItemSelection);
                return new BotResponse
                {
                    Message = $"""【{context.CurrentDestination}】請輸入要編輯的項目編號或"返回主選單"。"""
                };

            case BotCommand.Delete:
                _stateManager.SetState(userId, ConversationState.DeleteFlow);
                return new BotResponse
                {
                    Message = $"""【{context.CurrentDestination}】請輸入要刪除的項目編號或"返回主選單"。"""
                };

            case BotCommand.SwitchDestination:
                _stateManager.SetState(userId, ConversationState.SwitchDestination);
                var destinations = await _queryService.GetAvailableDestinationsAsync();
                var destListMsg = destinations.Any()
                    ? "\n已建立的地區：\n" + string.Join("\n", destinations.Select((d, idx) => $"{idx + 1}. {d}"))
                    : "\n目前尚無其他地區。";

                return new BotResponse
                {
                    Message = $"目前選定地區為【{context.CurrentDestination}】。{destListMsg}\n\n請輸入號碼選擇，或直接輸入新的地區名稱（如：東京、首爾、曼谷），或輸入「返回主選單」取消："
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
                    Message = $"請輸入 儲存、查詢、編輯、刪除、切換地區 或輸入「再見小幫手」退出。"
                };
        }
    }

    private async Task<BotResponse> HandleSwitchDestinationAsync(
        string userId,
        string message,
        ConversationContext context)
    {
        var input = message.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            return new BotResponse
            {
                Message = "請輸入有效的地區名稱或號碼。"
            };
        }

        var destinations = await _queryService.GetAvailableDestinationsAsync();

        string chosenDestination = input;
        if (int.TryParse(input, out var index) && index >= 1 && index <= destinations.Count)
        {
            chosenDestination = destinations[index - 1];
        }

        context.CurrentDestination = chosenDestination;
        _stateManager.SetState(userId, ConversationState.MainMenu);

        return new BotResponse
        {
            Message = $"已切換目前地區為【{chosenDestination}】！📍\n\n{GetWakeUpNotice(chosenDestination)}"
        };
    }
}
