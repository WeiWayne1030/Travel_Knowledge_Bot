using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Flows;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.State;

namespace OkinawaBot.Application.Handlers;

public class MessageHandler
{
    private readonly ConversationStateManager _stateManager;
    private readonly BotCommandParser _commandParser;
    private readonly SaveFlowHandler _saveFlowHandler;
    private readonly QueryFlowHandler _queryFlowHandler;
    private readonly EditFlowHandler _editFlowHandler;
    private readonly DeleteFlowHandler _deleteFlowHandler;

    public MessageHandler(
        ConversationStateManager stateManager,
        BotCommandParser commandParser,
        SaveFlowHandler saveFlowHandler,
        QueryFlowHandler queryFlowHandler,
        EditFlowHandler editFlowHandler,
        DeleteFlowHandler deleteFlowHandler)
    {
        _stateManager = stateManager;
        _commandParser = commandParser;
        _saveFlowHandler = saveFlowHandler;
        _queryFlowHandler = queryFlowHandler;
        _editFlowHandler = editFlowHandler;
        _deleteFlowHandler = deleteFlowHandler;
    }

    public async Task<BotResponse> HandleAsync(
        string userId,
        string message)
    {
        var context =
            _stateManager.GetOrCreate(userId);

        if (context.State ==
            ConversationState.MainMenu)
        {
            return HandleMainMenu(
                userId,
                message);
        }

        if (context.State ==
                ConversationState.SaveFlow ||
            context.State ==
                ConversationState.SaveDuplicateConfirmation)
        {
            return await _saveFlowHandler.HandleAsync(
                userId,
                message);
        }

        if (context.State ==
            ConversationState.QueryFlow)
        {
            return await _queryFlowHandler.HandleAsync(
                userId,
                message);
        }

        if (context.State ==
                ConversationState.EditItemSelection ||
            context.State ==
                ConversationState.EditDataInput ||
            context.State ==
                ConversationState.EditDuplicateConfirmation)
        {
            return await _editFlowHandler.HandleAsync(
                userId,
                message);
        }

        if (context.State == ConversationState.DeleteFlow ||
        context.State == ConversationState.DeleteConfirmation)
        {
            return await _deleteFlowHandler.HandleAsync(
                userId,
                message);
        }

        return new BotResponse
        {
            Message = "目前無法處理這個操作。"
        };
    }

    private BotResponse HandleMainMenu(
        string userId,
        string message)
    {
        var command =
            _commandParser.Parse(message);

        switch (command)
        {
            case BotCommand.Save:
                _stateManager.SetState(
                    userId,
                    ConversationState.SaveFlow);

                return new BotResponse
                {
                    Message =
                        "請輸入旅遊資訊：\n" +
                        "URL\n" +
                        "#Category\n" +
                        "Name"
                };

            case BotCommand.Query:
                _stateManager.SetState(
                    userId,
                    ConversationState.QueryFlow);

                return new BotResponse
                {
                    Message =
                        "請輸入要查詢的 Category。"
                };

            case BotCommand.Edit:
                _stateManager.SetState(
                    userId,
                    ConversationState.EditItemSelection);

                return new BotResponse
                {
                    Message =
                        "請輸入要編輯的項目編號。"
                };

            case BotCommand.Delete:
                _stateManager.SetState(
                    userId,
                    ConversationState.DeleteFlow);

                return new BotResponse
                {
                    Message =
                        "請輸入要刪除的項目編號。"
                };

            default:
                return new BotResponse
                {
                    Message =
                        "請輸入 Save、Query、Edit 或 Delete。"
                };
        }
    }
}