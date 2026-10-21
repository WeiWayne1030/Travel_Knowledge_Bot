using MediatR;
using OkinawaBot.Application.Requests;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;

namespace OkinawaBot.Application.Flows;

public class DeleteFlowHandler : IRequestHandler<DeleteFlowCommand, BotResponse>
{
    private readonly ConversationStateManager _stateManager;
    private readonly DeleteService _deleteService;

    public DeleteFlowHandler(
        ConversationStateManager stateManager,
        DeleteService deleteService)
    {
        _stateManager = stateManager;
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
            Message = BotMessages.OperationNotSupported
        };
    }

    private async Task<BotResponse> HandleItemSelectionAsync(
    string userId,
    string message,
    ConversationContext context)
    {
        var items =
            await _deleteService.GetDeletableItemsAsync(context.CurrentDestination);

        if (items.Count == 0)
        {
            _stateManager.ResetToMainMenu(userId);

            return new BotResponse
            {
                Message = BotMessages.NoItemsToProcess("刪除")
            };
        }

        if (!int.TryParse(message.Trim(), out var number))
        {
            return new BotResponse
            {
                Message = BotMessages.InvalidItemNumber
            };
        }

        if (number < 1 || number > items.Count)
        {
            return new BotResponse
            {
                Message = BotMessages.ItemNumberOutOfRange(items.Count)
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
                BotMessages.ContinueOrReturnPrompt
        };
    }

    private async Task<BotResponse> HandleConfirmationAsync(
    string userId,
    string message,
    ConversationContext context)
    {
        if (!string.Equals(
                message.Trim(),
                "繼續",
                StringComparison.OrdinalIgnoreCase))
        {
            return new BotResponse
            {
                Message = BotMessages.ContinueOrReturnPrompt
            };
        }

        if (context.CurrentItemId is null)
        {
            _stateManager.ResetToMainMenu(userId);

            return new BotResponse
            {
                Message = BotMessages.WithMainMenu("找不到要刪除的項目，操作已取消。")
            };
        }

        var success =
            await _deleteService.DeleteAsync(
                context.CurrentItemId.Value);

        if (!success)
        {
            _stateManager.ResetToMainMenu(userId);

            return new BotResponse
            {
                Message = BotMessages.WithMainMenu("找不到這筆資料，可能已經被刪除。")
            };
        }

        _stateManager.ResetToMainMenu(userId);

        return new BotResponse
        {
            Message = BotMessages.WithMainMenu("旅遊資訊刪除成功。")
        };
    }
}