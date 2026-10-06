using MediatR;
using OkinawaBot.Application.Requests;
using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Input;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;
using OkinawaBot.Domain.Interfaces;

namespace OkinawaBot.Application.Flows;

public class EditFlowHandler : IRequestHandler<EditFlowCommand, BotResponse>
{
    private readonly ConversationStateManager _stateManager;
    private readonly BotCommandParser _commandParser;
    private readonly EditService _editService;
    private readonly SaveInputParser _inputParser;

    public EditFlowHandler(
        ConversationStateManager stateManager,
        BotCommandParser commandParser,
        EditService editService,
        SaveInputParser inputParser)
    {
        _stateManager = stateManager;
        _commandParser = commandParser;
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

    private async Task<BotResponse> HandleDataInputAsync(
    string userId,
    string message,
    ConversationContext context)
    {
        if (context.CurrentItemId is null)
        {
            _stateManager.Reset(userId);

            return new BotResponse
            {
                Message = "找不到目前要編輯的資料，請重新操作。"
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
            Url = parseResult.Url!,
            Category = parseResult.Category!,
            Name = parseResult.Name!
        };

        var result =
            await _editService.UpdateAsync(
                context.CurrentItemId.Value,
                request);

        if (result.Status == EditResultStatus.NotFound)
        {
            _stateManager.Reset(userId);

            return new BotResponse
            {
                Message = "找不到要編輯的資料。"
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
                    "如果仍然要儲存，請輸入 Continue。\n" +
                    "如果不要修改，請輸入 Return。"
            };
        }

        _stateManager.Reset(userId);

        return new BotResponse
        {
            Message = "旅遊資訊修改成功。"
        };
    }

    private async Task<BotResponse>
    HandleDuplicateConfirmationAsync(
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
                Message = "已取消修改，回到主選單。"
            };
        }

        if (!message.Trim()
            .Equals("Continue", StringComparison.OrdinalIgnoreCase))
        {
            return new BotResponse
            {
                Message =
                    "請輸入 Continue 或 Return。"
            };
        }

        if (context.CurrentItemId is null ||
            context.PendingEdit is null)
        {
            _stateManager.Reset(userId);

            return new BotResponse
            {
                Message =
                    "找不到待處理的修改資料，請重新操作。"
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
            _stateManager.Reset(userId);

            return new BotResponse
            {
                Message = "找不到要編輯的資料。"
            };
        }

        _stateManager.Reset(userId);

        return new BotResponse
        {
            Message = "旅遊資訊修改成功。"
        };
    }
}
