using OkinawaBot.Application.Models;

namespace OkinawaBot.Application.State;

//流程中需要卡控的項目
public class ConversationContext
{
    public string UserId { get; set; } = string.Empty;

    public ConversationState State { get; set; }
        = ConversationState.Idle;

    public int? CurrentItemId { get; set; }

    public SaveTravelItemRequest? PendingSave { get; set; }

    public UpdateTravelItemRequest? PendingEdit { get; set; }
}