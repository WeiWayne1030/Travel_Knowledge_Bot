using MediatR;
using OkinawaBot.Application.Requests;
using OkinawaBot.Application.Input;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;
using OkinawaBot.Domain.Interfaces;

namespace OkinawaBot.Application.Flows;

public class EditFlowHandler : IRequestHandler<EditFlowCommand, BotResponse>
{
    private readonly ConversationStateManager _stateManager;
    private readonly EditService _editService;
    private readonly EditInputParser _inputParser;

    public EditFlowHandler(
        ConversationStateManager stateManager,
        EditService editService,
        EditInputParser inputParser)
    {
        _stateManager = stateManager;
        _editService = editService;
        _inputParser = inputParser;
    }

    public async Task<BotResponse> Handle(
        EditFlowCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var message = request.Message;

        var context =
            _stateManager.GetOrCreate(userId);

        if (context.State ==
            ConversationState.EditItemSelection)
        {
            return await HandleItemSelectionAsync(
                userId,
                message,
                context);
        }

        if (context.State ==
        ConversationState.EditDataInput)
            {
                return await HandleDataInputAsync(
                    userId,
                    message,
                    context);
            }

        if (context.State ==
        ConversationState.EditDuplicateConfirmation)
            {
                return await HandleDuplicateConfirmationAsync(
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
            await _editService.GetEditableItemsAsync(context.CurrentDestination);

        if (items.Count == 0)
        {
            _stateManager.ResetToMainMenu(userId);

            return new BotResponse
            {
                Message =
                    $"目前沒有可以編輯的資料。\n\n{BotMessages.MainMenuPrompt}"
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
                $"目前資料：\n" +
                $"名稱：{selectedItem.Name}\n" +
                $"類別：#{selectedItem.Category}\n" +
                $"網址：{selectedItem.Url}\n\n" +
                "請輸入要修改的內容（可任一修改或同時修改多項）：\n" +
                "• 僅改網址：輸入新 URL（例如 https://...）\n" +
                "• 僅改類別：輸入 #新類別（例如 #美食）\n" +
                "• 僅改名稱：輸入新名稱（例如 美麗海水族館 或 名稱: 美麗海水族館）\n" +
                "• 多項修改：URL #新類別 新名稱"
        };
    }

    private async Task<BotResponse> HandleDataInputAsync(
    string userId,
    string message,
    ConversationContext context)
    {
        if (context.CurrentItemId is null)
        {
            _stateManager.ResetToMainMenu(userId);

            return new BotResponse
            {
                Message = $"找不到目前要編輯的資料，請重新操作。\n\n{BotMessages.MainMenuPrompt}"
            };
        }

        var parseResult =
            _inputParser.Parse(message);

        if (!parseResult.IsSuccess)
        {
            return new BotResponse
            {
                Message = parseResult.ErrorMessage
                    ?? "輸入格式錯誤。"
            };
        }

        var request = new UpdateTravelItemRequest
        {
            Url = parseResult.Url,
            Category = parseResult.Category,
            Name = parseResult.Name
        };

        var result =
            await _editService.UpdateAsync(
                context.CurrentItemId.Value,
                request);

        if (result.Status == EditResultStatus.NotFound)
        {
            _stateManager.ResetToMainMenu(userId);

            return new BotResponse
            {
                Message = $"找不到要編輯的資料。\n\n{BotMessages.MainMenuPrompt}"
            };
        }
        if (result.Status == EditResultStatus.Duplicate)
        {
            context.PendingEdit = request;

            context.State =
                ConversationState.EditDuplicateConfirmation;

            return new BotResponse
            {
                Message =
                    $"名稱「{request.Name}」已經存在。\n\n" +
                    "如果仍然要儲存，請輸入 繼續。\n" +
                    "如果不要修改，請輸入 返回主選單。"
            };
        }

        _stateManager.ResetToMainMenu(userId);

        return new BotResponse
        {
            Message = $"旅遊資訊修改成功。\n\n{BotMessages.MainMenuPrompt}"
        };
    }

    private async Task<BotResponse>
    HandleDuplicateConfirmationAsync(
        string userId,
        string message,
        ConversationContext context)
    {
        if (!message.Trim()
            .Equals("繼續", StringComparison.OrdinalIgnoreCase))
        {
            return new BotResponse
            {
                Message =
                    "請輸入 繼續 或 返回主選單。"
            };
        }

        if (context.CurrentItemId is null ||
            context.PendingEdit is null)
        {
            _stateManager.ResetToMainMenu(userId);

            return new BotResponse
            {
                Message =
                    $"找不到待處理的修改資料，請重新操作。\n\n{BotMessages.MainMenuPrompt}"
            };
        }

        //允許重複name
        var result =
            await _editService.UpdateAsync(
                context.CurrentItemId.Value,
                context.PendingEdit,
                skipDuplicateCheck: true);

        if (result.Status == EditResultStatus.NotFound)
        {
            _stateManager.ResetToMainMenu(userId);

            return new BotResponse
            {
                Message = $"找不到要編輯的資料。\n\n{BotMessages.MainMenuPrompt}"
            };
        }

        _stateManager.ResetToMainMenu(userId);

        return new BotResponse
        {
            Message = $"旅遊資訊修改成功。\n\n{BotMessages.MainMenuPrompt}"
        };
    }
}
