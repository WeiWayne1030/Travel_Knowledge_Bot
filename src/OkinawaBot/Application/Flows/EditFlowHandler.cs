using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;
using OkinawaBot.Domain.Interfaces;

namespace OkinawaBot.Application.Flows;

public class EditFlowHandler
{
    private readonly ConversationStateManager _stateManager;
    private readonly BotCommandParser _commandParser;
    private readonly EditService _editService;

    public EditFlowHandler(
        ConversationStateManager stateManager,
        BotCommandParser commandParser,
        EditService editService)
    {
        _stateManager = stateManager;
        _commandParser = commandParser;
        _editService = editService;
    }

    public async Task<BotResponse> HandleAsync(
        string userId,
        string message)
    {
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
            ConversationState.EditItemSelection)
        {
            return await HandleItemSelectionAsync(
                userId,
                message,
                context);
        }

        return new BotResponse
        {
            Message = "目前無法處理這個操作。"
        };
    }

    private async Task<BotResponse>
        HandleItemSelectionAsync(
            string userId,
            string message,
            ConversationContext context)
    {
        var items =
            await _editService.GetEditableItemsAsync();

        if (items.Count == 0)
        {
            _stateManager.Reset(userId);

            return new BotResponse
            {
                Message =
                    "目前沒有可以編輯的資料。"
            };
        }

        if (!int.TryParse(
            message.Trim(),
            out var selection))
        {
            return new BotResponse
            {
                Message =
                    "請輸入有效的項目編號。"
            };
        }

        if (selection < 1 ||
            selection > items.Count)
        {
            return new BotResponse
            {
                Message =
                    $"請輸入 1 到 {items.Count} 之間的項目編號。"
            };
        }

        var selectedItem =
            items[selection - 1];

        context.CurrentItemId =
            selectedItem.Id;

        context.State =
            ConversationState.EditDataInput;

        return new BotResponse
        {
            Message =
                $"你選擇了「{selectedItem.Name}」。\n\n" +
                "請輸入新的資料：\n" +
                "URL\n" +
                "#Category\n" +
                "Name"
        };
    }
}
