using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Flows;
using OkinawaBot.Application.Input;
using OkinawaBot.Application.State;


namespace OkinawaBot.Application.Handlers;

public class MessageHandler
{
    private readonly ConversationStateManager _stateManager;
    private readonly BotCommandParser _commandParser;
    private readonly SaveFlowHandler _saveFlowHandler;
    private readonly QueryFlowHandler _queryFlowHandler;

    public MessageHandler(
        ConversationStateManager stateManager,
        BotCommandParser commandParser,
        SaveFlowHandler saveFlowHandler,
        QueryFlowHandler queryFlowHandler)
    {
        _stateManager = stateManager;
        _commandParser = commandParser;
        _saveFlowHandler = saveFlowHandler;
        _queryFlowHandler = queryFlowHandler;
    }

    public Task HandleAsync(string userId, string message)
    {
        var context = _stateManager.GetOrCreate(userId);

        if (context.State == ConversationState.MainMenu)
        {
            return HandleMainMenuAsync(
                userId,
                message);
        }

        if (context.State == ConversationState.SaveFlow || context.State == ConversationState.SaveDuplicateConfirmation)
        {
            return _saveFlowHandler.HandleAsync(
                userId,
                message);
        }

        if (context.State == ConversationState.QueryFlow)
        {
            return _queryFlowHandler.HandleAsync(
               userId,
               message);
        }

        return Task.CompletedTask;
    }

    private Task HandleMainMenuAsync(string userId, string message)
    {
        var command = _commandParser.Parse(message);

        switch (command)
        {
            case BotCommand.Save:

                _stateManager.SetState(
                    userId,
                    ConversationState.SaveFlow);

                break;

            case BotCommand.Query:

                _stateManager.SetState(
                    userId,
                    ConversationState.QueryFlow);

                break;

            case BotCommand.Edit:

                _stateManager.SetState(
                    userId,
                    ConversationState.EditItemSelection);

                break;

            case BotCommand.Delete:

                _stateManager.SetState(
                    userId,
                    ConversationState.DeleteFlow);

                break;
        }

        return Task.CompletedTask;
    }
}