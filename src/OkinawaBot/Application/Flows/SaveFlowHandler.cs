using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Input;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;

namespace OkinawaBot.Application.Flows;

public class SaveFlowHandler
{
    private readonly ConversationStateManager _stateManager;

    private readonly BotCommandParser _commandParser;

    private readonly SaveInputParser _inputParser;

    private readonly SaveService _saveService;

    public SaveFlowHandler(
        ConversationStateManager stateManager,
        BotCommandParser commandParser,
        SaveInputParser inputParser,
        SaveService saveService)
    {
        _stateManager = stateManager;
        _commandParser = commandParser;
        _inputParser = inputParser;
        _saveService = saveService;
    }

    public async Task<BotResponse> HandleAsync(
        string userId,
        string message)
    {

        var context =
        _stateManager.GetOrCreate(userId);

        //判斷state是不是SaveDuplicateConfirmation
        if (context.State ==
            ConversationState.SaveDuplicateConfirmation)
        {
            return await HandleDuplicateConfirmationAsync(
                userId,
                message,
                context);
        }

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

        var parseResult =
            _inputParser.Parse(message);

        if (!parseResult.IsSuccess)
        {
            return new BotResponse
            {
                Message = parseResult.ErrorMessage!
            };
        }

        var request =
            new SaveTravelItemRequest
            {
                Url = parseResult.Url!,
                Category = parseResult.Category!,
                Name = parseResult.Name!
            };

        var result = await _saveService.SaveAsync(request);

        if (result.Status == SaveResultStatus.Success)
        {
            _stateManager.Reset(userId);

            return new BotResponse
            {
                Message = result.Message
            };
        }

        if (result.Status == SaveResultStatus.Duplicate)
        {

            context.PendingSave = request;

            _stateManager.SetState(userId,  ConversationState.SaveDuplicateConfirmation);

            return new BotResponse
            {
                Message =
                    $"名稱「{request.Name}」已經存在，是否仍要儲存？\n" +
                    "請輸入 Continue 或 Return。"
            };
        }

        return new BotResponse
        {
            Message = result.Message
        };
    }

    private async Task<BotResponse> HandleDuplicateConfirmationAsync(
        string userId,
        string message,
        ConversationContext context)
    {
        if (message.Equals(
            "Return",
            StringComparison.OrdinalIgnoreCase))
        {
            context.PendingSave = null;

            _stateManager.Reset(userId);

            return new BotResponse
            {
                Message = "已取消儲存並回到主選單。"
            };
        }

        if (message.Equals(
            "Continue",
            StringComparison.OrdinalIgnoreCase))
        {
            if (context.PendingSave is null)
            {
                _stateManager.Reset(userId);

                return new BotResponse
                {
                    Message = "找不到待確認的資料，已回到主選單。"
                };
            }

            var result =
                await _saveService.SaveAsync(
                    context.PendingSave,
                    skipDuplicateCheck: true);

            if (result.Status == SaveResultStatus.Success)
            {
                context.PendingSave = null;

                _stateManager.Reset(userId);

                return new BotResponse
                {
                    Message = result.Message
                };
            }

            return new BotResponse
            {
                Message = result.Message
            };
        }

        return new BotResponse
        {
            Message = "請輸入 Continue 或 Return。"
        };
    }
}