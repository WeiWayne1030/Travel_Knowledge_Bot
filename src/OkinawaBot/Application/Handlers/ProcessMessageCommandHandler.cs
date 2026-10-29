using MediatR;
using Microsoft.AspNetCore.Http;
using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Requests;
using OkinawaBot.Application.State;

namespace OkinawaBot.Application.Handlers;

public class ProcessMessageCommandHandler : IRequestHandler<ProcessMessageCommand, BotResponse>
{
    private static string GetWakeUpNotice(string destination) =>
        $"您好！我是旅遊小幫手。\n目前地區：【{destination}】 📍\n{BotMessages.MainMenuPrompt}";

    private readonly ConversationStateManager _stateManager;
    private readonly BotCommandParser _commandParser;
    private readonly IMediator _mediator;
    private readonly OkinawaBot.Application.Services.QueryService _queryService;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public ProcessMessageCommandHandler(
        ConversationStateManager stateManager,
        BotCommandParser commandParser,
        IMediator mediator,
        OkinawaBot.Application.Services.QueryService queryService,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _stateManager = stateManager;
        _commandParser = commandParser;
        _mediator = mediator;
        _queryService = queryService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<BotResponse> Handle(ProcessMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var message = request.Message;
        var command = _commandParser.Parse(message);

        var context = _stateManager.GetOrCreate(userId);

        if (_httpContextAccessor?.HttpContext != null)
        {
            _httpContextAccessor.HttpContext.Items["UserId"] = userId;
            _httpContextAccessor.HttpContext.Items["ConversationState"] = context.State.ToString();
        }

        BotResponse response;

        // 檢查暫態流程是否逾時
        if (_stateManager.IsFlowExpired(context))
        {
            _stateManager.Reset(userId);

            if (command == BotCommand.WakeUp)
            {
                _stateManager.SetState(userId, ConversationState.MainMenu);
                response = new BotResponse
                {
                    Message = GetWakeUpNotice(context.CurrentDestination),
                    QuickReplyOptions = BotMessages.MainMenuQuickReplies
                };
            }
            else
            {
                response = new BotResponse
                {
                    Message = BotMessages.FlowTimeoutNotice,
                    QuickReplyOptions = BotMessages.WakeUpQuickReplies
                };
            }

            await _stateManager.SaveCurrentAsync();
            return response;
        }

        context.LastActiveAt = DateTime.UtcNow;

        if (context.State == ConversationState.Idle)
        {
            if (command == BotCommand.WakeUp)
            {
                _stateManager.SetState(userId, ConversationState.MainMenu);
                response = new BotResponse
                {
                    Message = GetWakeUpNotice(context.CurrentDestination),
                    QuickReplyOptions = BotMessages.MainMenuQuickReplies
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
        else if (command == BotCommand.Sleep)
        {
            _stateManager.Reset(userId);
            response = new BotResponse
            {
                Message = BotMessages.SleepNotice,
                QuickReplyOptions = BotMessages.WakeUpQuickReplies
            };
        }
        else if (command == BotCommand.Return)
        {
            _stateManager.ResetToMainMenu(userId);
            response = new BotResponse
            {
                Message = GetWakeUpNotice(context.CurrentDestination),
                QuickReplyOptions = BotMessages.MainMenuQuickReplies
            };
        }
        else if (context.State == ConversationState.MainMenu)
        {
            response = await HandleMainMenuAsync(userId, message, context, command);
        }
        else if (context.State == ConversationState.SwitchDestination)
        {
            response = await HandleSwitchDestinationAsync(userId, message, context);
        }
        else if (context.State == ConversationState.SwitchDestinationConfirmation)
        {
            response = HandleSwitchDestinationConfirmation(userId, message, context);
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
                Message = BotMessages.OperationNotSupported,
                QuickReplyOptions = BotMessages.ReturnOrSleepQuickReplies
            };
        }

        if (_httpContextAccessor?.HttpContext != null)
        {
            _httpContextAccessor.HttpContext.Items["ConversationState"] = context.State.ToString();
        }

        await _stateManager.SaveCurrentAsync();
        return response;
    }

    private async Task<BotResponse> HandleMainMenuAsync(
        string userId,
        string message,
        ConversationContext context,
        BotCommand command)
    {
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
                        "名稱\n\n" +
                        "（若要取消，請輸入「返回主選單」）",
                    QuickReplyOptions = BotMessages.ReturnOrSleepQuickReplies
                };

            case BotCommand.Query:
                _stateManager.SetState(userId, ConversationState.QueryFlow);
                var categories = await _queryService.GetAvailableCategoriesAsync(context.CurrentDestination);
                var categoriesMsg = categories.Any() 
                    ? $"\n【{context.CurrentDestination}】目前有的類別：\n" + string.Join("\n", categories)
                    : $"\n【{context.CurrentDestination}】目前還沒有任何類別。";

                var queryOptions = categories.Take(11).ToList();
                queryOptions.Add("返回主選單");

                return new BotResponse
                {
                    Message = $"""請輸入要查詢的類別或"返回主選單"。{categoriesMsg}""",
                    QuickReplyOptions = queryOptions
                };

            case BotCommand.Edit:
                _stateManager.SetState(userId, ConversationState.EditItemSelection);
                var editableItems = await _queryService.GetAllItemsAsync(context.CurrentDestination);
                var editOptions = editableItems.Count > 0 && editableItems.Count <= 11
                    ? Enumerable.Range(1, editableItems.Count).Select(i => i.ToString()).Concat(new[] { "返回主選單" }).ToList()
                    : (IReadOnlyList<string>)BotMessages.ReturnOrSleepQuickReplies;

                return new BotResponse
                {
                    Message = $"""【{context.CurrentDestination}】請輸入要編輯的項目編號或"返回主選單"。{BotMessages.ItemList("編輯", editableItems)}""",
                    QuickReplyOptions = editOptions
                };

            case BotCommand.Delete:
                _stateManager.SetState(userId, ConversationState.DeleteFlow);
                var deletableItems = await _queryService.GetAllItemsAsync(context.CurrentDestination);
                var deleteOptions = deletableItems.Count > 0 && deletableItems.Count <= 11
                    ? Enumerable.Range(1, deletableItems.Count).Select(i => i.ToString()).Concat(new[] { "返回主選單" }).ToList()
                    : (IReadOnlyList<string>)BotMessages.ReturnOrSleepQuickReplies;

                return new BotResponse
                {
                    Message = $"""【{context.CurrentDestination}】請輸入要刪除的項目編號或"返回主選單"。{BotMessages.ItemList("刪除", deletableItems)}""",
                    QuickReplyOptions = deleteOptions
                };

            case BotCommand.SwitchDestination:
                _stateManager.SetState(userId, ConversationState.SwitchDestination);
                var destinations = await _queryService.GetAvailableDestinationsAsync();
                var destListMsg = destinations.Any()
                    ? "\n已建立的地區：\n" + string.Join("\n", destinations.Select((d, idx) => $"{idx + 1}. {d}"))
                    : "\n目前尚無其他地區。";

                var switchOptions = destinations.Take(11).ToList();
                switchOptions.Add("返回主選單");

                return new BotResponse
                {
                    Message = $"目前選定地區為【{context.CurrentDestination}】。{destListMsg}\n\n請輸入號碼選擇，或直接輸入新的地區名稱（如：東京、首爾、曼谷），或輸入「返回主選單」取消：",
                    QuickReplyOptions = switchOptions
                };

            case BotCommand.Sleep:
                _stateManager.Reset(userId);
                return new BotResponse
                {
                    Message = BotMessages.SleepNotice,
                    QuickReplyOptions = BotMessages.WakeUpQuickReplies
                };

            default:
                return new BotResponse
                {
                    Message = BotMessages.MainMenuPrompt,
                    QuickReplyOptions = BotMessages.MainMenuQuickReplies
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
                Message = "請輸入有效的地區名稱或號碼，或輸入「返回主選單」取消。",
                QuickReplyOptions = BotMessages.ReturnOrSleepQuickReplies
            };
        }

        var destinations = await _queryService.GetAvailableDestinationsAsync();

        if (int.TryParse(input, out var index) && index >= 1 && index <= destinations.Count)
        {
            var chosen = destinations[index - 1];
            context.CurrentDestination = chosen;
            _stateManager.SetState(userId, ConversationState.MainMenu);

            return new BotResponse
            {
                Message = $"已切換目前地區為【{chosen}】！📍\n\n{GetWakeUpNotice(chosen)}",
                QuickReplyOptions = BotMessages.MainMenuQuickReplies
            };
        }

        var matchedDestination = destinations.FirstOrDefault(
            d => string.Equals(d, input, StringComparison.OrdinalIgnoreCase));

        if (matchedDestination != null)
        {
            context.CurrentDestination = matchedDestination;
            _stateManager.SetState(userId, ConversationState.MainMenu);

            return new BotResponse
            {
                Message = $"已切換目前地區為【{matchedDestination}】！📍\n\n{GetWakeUpNotice(matchedDestination)}",
                QuickReplyOptions = BotMessages.MainMenuQuickReplies
            };
        }

        // 新地區防手滑二次確認
        context.PendingDestination = input;
        _stateManager.SetState(userId, ConversationState.SwitchDestinationConfirmation);

        return new BotResponse
        {
            Message = $"偵測到新地區「{input}」。\n確定要新增並切換到此地區嗎？\n{BotMessages.ContinueOrReturnPrompt}",
            QuickReplyOptions = BotMessages.ContinueOrReturnQuickReplies
        };
    }

    private BotResponse HandleSwitchDestinationConfirmation(
        string userId,
        string message,
        ConversationContext context)
    {
        if (!string.Equals(message.Trim(), "繼續", StringComparison.OrdinalIgnoreCase))
        {
            return new BotResponse
            {
                Message = BotMessages.ContinueOrReturnPrompt,
                QuickReplyOptions = BotMessages.ContinueOrReturnQuickReplies
            };
        }

        if (string.IsNullOrWhiteSpace(context.PendingDestination))
        {
            _stateManager.ResetToMainMenu(userId);
            return new BotResponse
            {
                Message = GetWakeUpNotice(context.CurrentDestination),
                QuickReplyOptions = BotMessages.MainMenuQuickReplies
            };
        }

        var newDestination = context.PendingDestination;
        context.CurrentDestination = newDestination;
        context.PendingDestination = null;
        _stateManager.SetState(userId, ConversationState.MainMenu);

        return new BotResponse
        {
            Message = $"已切換目前地區為【{newDestination}】！📍\n\n{GetWakeUpNotice(newDestination)}",
            QuickReplyOptions = BotMessages.MainMenuQuickReplies
        };
    }
}
