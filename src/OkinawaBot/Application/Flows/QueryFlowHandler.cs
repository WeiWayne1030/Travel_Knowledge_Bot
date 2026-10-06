using System.Text;
using MediatR;
using OkinawaBot.Application.Requests;
using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;

namespace OkinawaBot.Application.Flows;

public class QueryFlowHandler : IRequestHandler<QueryFlowCommand, BotResponse>
{
    private readonly ConversationStateManager _stateManager;
    private readonly BotCommandParser _commandParser;
    private readonly QueryService _queryService;

    public QueryFlowHandler(
        ConversationStateManager stateManager,
        BotCommandParser commandParser,
        QueryService queryService)
    {
        _stateManager = stateManager;
        _commandParser = commandParser;
        _queryService = queryService;
    }

    public async Task<BotResponse> Handle(
        QueryFlowCommand request,
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

        var category = message.Trim();

        if (string.IsNullOrWhiteSpace(category))
        {
            return new BotResponse
            {
                Message = "類別不可為空"
            };
        }

        var items =
            await _queryService.QueryByCategoryAsync(
                category);

        if (items.Count == 0)
        {
            return new BotResponse
            {
                Message =
                    $"找不到類別「{category}」的資料，請重新輸入。"
            };
        }

        var response =
            new StringBuilder();

        response.AppendLine(
            $"【{category}】");

        for (var i = 0; i < items.Count; i++)
        {
            response.AppendLine();
            response.AppendLine(
                $"{i + 1}. {items[i].Name}");
            response.AppendLine(
                $"   {items[i].Url}");
        }

        _stateManager.Reset(userId);

        return new BotResponse
        {
            Message = response.ToString()
        };
    }
}