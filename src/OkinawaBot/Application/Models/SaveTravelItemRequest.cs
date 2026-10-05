namespace OkinawaBot.Application.Models;

//儲存資料
public class SaveTravelItemRequest
{
    public string Url { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
