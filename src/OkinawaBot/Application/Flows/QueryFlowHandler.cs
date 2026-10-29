using System.Text;
using MediatR;
using OkinawaBot.Application.Requests;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;

namespace OkinawaBot.Application.Flows;

public class QueryFlowHandler : IRequestHandler<QueryFlowCommand, BotResponse>
{
    private readonly ConversationStateManager _stateManager;
    private readonly QueryService _queryService;

    public QueryFlowHandler(
        ConversationStateManager stateManager,
        QueryService queryService)
    {
        _stateManager = stateManager;
        _queryService = queryService;
    }

    public async Task<BotResponse> Handle(
        QueryFlowCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var message = request.Message;

        var category = message.Trim();

        if (string.IsNullOrWhiteSpace(category))
        {
            return new BotResponse
            {
                Message = "類別不可為空，請輸入欲查詢的類別，或輸入「返回主選單」。",
                QuickReplyOptions = BotMessages.ReturnOrSleepQuickReplies
            };
        }

        var context =
            _stateManager.GetOrCreate(userId);

        var items =
            await _queryService.QueryByCategoryAsync(
                category, context.CurrentDestination);

        if (items.Count == 0)
        {
            return new BotResponse
            {
                Message =
                    $"在【{context.CurrentDestination}】找不到類別「{category}」的資料，請重新輸入，或輸入「返回主選單」。",
                QuickReplyOptions = BotMessages.ReturnOrSleepQuickReplies
            };
        }

        var response =
            new StringBuilder();

        response.AppendLine(
            $"📍【{context.CurrentDestination}】 【{category}】");

        for (var i = 0; i < items.Count; i++)
        {
            response.AppendLine();
            response.AppendLine(
                $"{i + 1}. {items[i].Name}");
            response.AppendLine(
                $"   {items[i].Url}");
        }

        _stateManager.ResetToMainMenu(userId);

        response.AppendLine();
        response.AppendLine("已為您回到主選單。");
        response.Append(BotMessages.MainMenuPrompt);

        return new BotResponse
        {
            Message = response.ToString(),
            QuickReplyOptions = BotMessages.MainMenuQuickReplies
        };
    }
}