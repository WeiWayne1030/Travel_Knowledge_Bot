using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Flows;
using OkinawaBot.Application.Models;
using OkinawaBot.Application.State;

namespace OkinawaBot.Application.Handlers;

public class MessageHandler
{
    private const string MainMenuMessage =
        "請輸入以下指令：\n" +
        "Save：儲存旅遊資訊\n" +
        "Query：查詢旅遊資訊\n" +
        "Edit：編輯旅遊資訊\n" +
        "Delete：刪除旅遊資訊";

    private const string SaveFormatMessage =
        "請依照以下格式輸入：\n" +
        "URL #分類 名稱\n" +
        "範例：https://example.com/okinawa #Attraction 美麗海水族館\n" +
        "輸入 Return 可回到主選單。";

    private const string NotAvailableMessage =
        "此功能尚未開放，請輸入 Return 回到主選單。";

    private readonly ConversationStateManager _stateManager;
    private readonly BotCommandParser _commandParser;
    private readonly SaveFlowHandler _saveFlowHandler;

    public MessageHandler(
        ConversationStateManager stateManager,
        BotCommandParser commandParser,
        SaveFlowHandler saveFlowHandler)
    {
        _stateManager = stateManager;
        _commandParser = commandParser;
        _saveFlowHandler = saveFlowHandler;
    }

    public Task<BotResponse> HandleAsync(string userId, string message)
    {
        var context = _stateManager.GetOrCreate(userId);

        if (context.State == ConversationState.MainMenu)
        {
            return Task.FromResult(
                HandleMainMenu(
                    userId,
                    message));
        }

        if (context.State == ConversationState.SaveFlow || context.State == ConversationState.SaveDuplicateConfirmation)
        {
            return _saveFlowHandler.HandleAsync(
                userId,
                message);
        }

        //Query / Edit / Delete 流程尚未實作，先支援 Return，避免使用者卡在該狀態
        return Task.FromResult(
            HandleNotAvailableFlow(
                userId,
                message));
    }

    private BotResponse HandleMainMenu(string userId, string message)
    {
        var command = _commandParser.Parse(message);

        switch (command)
        {
            case BotCommand.Save:

                _stateManager.SetState(
                    userId,
                    ConversationState.SaveFlow);

                return Reply(SaveFormatMessage);

            case BotCommand.Query:

                _stateManager.SetState(
                    userId,
                    ConversationState.QueryFlow);

                return Reply(NotAvailableMessage);

            case BotCommand.Edit:

                _stateManager.SetState(
                    userId,
                    ConversationState.EditFlow);

                return Reply(NotAvailableMessage);

            case BotCommand.Delete:

                _stateManager.SetState(
                    userId,
                    ConversationState.DeleteFlow);

                return Reply(NotAvailableMessage);

            default:

                return Reply(MainMenuMessage);
        }
    }

    private BotResponse HandleNotAvailableFlow(string userId, string message)
    {
        if (_commandParser.Parse(message) == BotCommand.Return)
        {
            _stateManager.Reset(userId);

            return Reply("已回到主選單。");
        }

        return Reply(NotAvailableMessage);
    }

    private static BotResponse Reply(string message)
    {
        return new BotResponse
        {
            Message = message
        };
    }
}