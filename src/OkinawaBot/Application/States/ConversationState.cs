namespace OkinawaBot.Application.State;

public enum ConversationState
{
    Idle,
    MainMenu,
    SwitchDestination,
    SwitchDestinationConfirmation,
    
    SaveFlow,
    SaveDuplicateConfirmation,
    
    QueryFlow,

    EditItemSelection,
    EditDataInput,
    EditDuplicateConfirmation,

    DeleteFlow,
    DeleteConfirmation,
}
