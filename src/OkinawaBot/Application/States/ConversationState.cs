namespace OkinawaBot.Application.State;

public enum ConversationState
{
    Idle,
    MainMenu,
    SwitchDestination,
    
    SaveFlow,
    SaveDuplicateConfirmation,
    
    QueryFlow,

    EditItemSelection,
    EditDataInput,
    EditDuplicateConfirmation,

    DeleteFlow,
    DeleteConfirmation,
}
