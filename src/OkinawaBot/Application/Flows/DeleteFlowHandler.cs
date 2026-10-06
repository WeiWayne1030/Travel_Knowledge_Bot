using MediatR;
using OkinawaBot.Application.Requests;
using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;

namespace OkinawaBot.Application.Flows;

public class DeleteFlowHandler : IRequestHandler<DeleteFlowCommand, BotResponse>
{
    private readonly ConversationStateManager _stateManager;
    private readonly BotCommandParser _commandParser;
    private readonly DeleteService _deleteService;

    public DeleteFlowHandler(
        ConversationStateManager stateManager,
        BotCommandParser commandParser,
        DeleteService deleteService)
    {
        _stateManager = stateManager;
        _commandParser = commandParser;
        _deleteService = deleteService;
    }

    public async Task<BotResponse> Handle(
        DeleteFlowCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var message = request.Message;

        var context =
            _stateManager.GetOrCreate(userId);

        var command =
            _commandParser.Parse(message);

        if (command == BotCommand.Return)
        {
            _stateManager.Reset(userId);

            return new BotResponse
            {
                Message = "已回到主選單。"
            };
        }

        if (context.State ==
            ConversationState.DeleteFlow)
        {
            return await HandleItemSelectionAsync(
                userId,
                message,
                context);
        }

        if (context.State ==
            ConversationState.DeleteConfirmation)
        {
            return await HandleConfirmationAsync(
                userId,
                message,
                context);
        }

        return new BotResponse
        {
            Message = "目前無法處理這個操作。"
        };
    }

    private async Task<BotResponse> HandleItemSelectionAsync(
    string userId,
    string message,
    ConversationContext context)
    {
        var items =
            await _deleteService.GetDeletableItemsAsync();

        if (items.Count == 0)
        {
            _stateManager.Reset(userId);

            return new BotResponse
            {
                Message = "目前沒有可以刪除的資料。"
            };
        }

        if (!int.TryParse(message.Trim(), out var number))
        {
            return new BotResponse
            {
                Message = "請輸入有效的項目編號。"
            };
        }

        if (number < 1 || number > items.Count)
        {
            return new BotResponse
            {
                Message =
                    $"請輸入 1 到 {items.Count} 之間的項目編號。"
            };
        }

        var selectedItem = items[number - 1];

        context.CurrentItemId = selectedItem.Id;

        context.State =
            ConversationState.DeleteConfirmation;

        return new BotResponse
        {
            Message =
                $"確定要刪除「{selectedItem.Name}」嗎？\n" +
                "請輸入 Continue 或 Return。"
        };
    }

    private async Task<BotResponse> HandleConfirmationAsync(
    string userId,
    string message,
    ConversationContext context)
    {
        var command =
            _commandParser.Parse(message);

        if (command == BotCommand.Return)
        {
            _stateManager.Reset(userId);

            return new BotResponse
            {
                Message = "已取消刪除。"
            };
        }

        if (!string.Equals(
                message.Trim(),
                "continue",
                StringComparison.OrdinalIgnoreCase))
        {
            return new BotResponse
            {
                Message =
                    "請輸入 Continue 或 Return。"
            };
        }

        if (context.CurrentItemId is null)
        {
            _stateManager.Reset(userId);

            return new BotResponse
            {
                Message =
                    "找不到要刪除的項目，操作已取消。"
            };
        }

        var success =
            await _deleteService.DeleteAsync(
                context.CurrentItemId.Value);

        if (!success)
        {
            _stateManager.Reset(userId);

            return new BotResponse
            {
                Message =
                    "找不到這筆資料，可能已經被刪除。"
            };
        }

        _stateManager.Reset(userId);

        return new BotResponse
        {
            Message = "旅遊資訊刪除成功。"
        };
    }
}