namespace OkinawaBot.Application.State;

public enum ConversationState
{
    Idle,
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
