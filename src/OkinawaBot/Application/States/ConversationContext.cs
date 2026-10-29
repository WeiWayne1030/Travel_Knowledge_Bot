using OkinawaBot.Application.Models;

namespace OkinawaBot.Application.State;

//流程中需要卡控的項目
public class ConversationContext
{
    public string UserId { get; set; } = string.Empty;

    public ConversationState State { get; set; }
        = ConversationState.Idle;

    public string CurrentDestination { get; set; } = "沖繩";

    public int? CurrentItemId { get; set; }

    public SaveTravelItemRequest? PendingSave { get; set; }
    
    public UpdateTravelItemRequest? PendingEdit { get; set; }

    public string? PendingDestination { get; set; }

    public DateTime? LastActiveAt { get; set; } = DateTime.UtcNow;
}