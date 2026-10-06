namespace OkinawaBot.Application.State;

public enum ConversationState
{
    MainMenu,
    
    SaveFlow,
    SaveDuplicateConfirmation,
    
    QueryFlow,

    EditItemSelection,
    EditDataInput,
    EditDuplicateConfirmation,

    DeleteFlow,
    DeleteConfirmation,
}
