using MediatR;
using OkinawaBot.Application.Requests;
using OkinawaBot.Application.Input;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;

namespace OkinawaBot.Application.Flows;

public class SaveFlowHandler : IRequestHandler<SaveFlowCommand, BotResponse>
{
    private readonly ConversationStateManager _stateManager;

    private readonly SaveInputParser _inputParser;

    private readonly SaveService _saveService;

    public SaveFlowHandler(
        ConversationStateManager stateManager,
        SaveInputParser inputParser,
        SaveService saveService)
    {
        _stateManager = stateManager;
        _inputParser = inputParser;
        _saveService = saveService;
    }

    public async Task<BotResponse> Handle(
        SaveFlowCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var message = request.Message;

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

        var parseResult =
            _inputParser.Parse(message);

        if (!parseResult.IsSuccess)
        {
            return new BotResponse
            {
                Message = parseResult.ErrorMessage!
            };
        }

        var saveRequest =
            new SaveTravelItemRequest
            {
                Destination = context.CurrentDestination,
                Url = parseResult.Url!,
                Category = parseResult.Category!,
                Name = parseResult.Name!
            };

        var result = await _saveService.SaveAsync(saveRequest);

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

            context.PendingSave = saveRequest;

            _stateManager.SetState(userId,  ConversationState.SaveDuplicateConfirmation);

            return new BotResponse
            {
                Message =
                    $"名稱「{saveRequest.Name}」已經存在，是否仍要儲存？\n" +
                    "請輸入 繼續 或 返回主選單。"
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
            "繼續",
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
            Message = "請輸入 繼續 或 返回主選單。"
        };
    }
}