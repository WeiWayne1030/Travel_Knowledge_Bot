using OkinawaBot.Application.Models;
using OkinawaBot.Domain.Entities;
using OkinawaBot.Domain.Interfaces;

namespace OkinawaBot.Application.Services;

public class SaveService
{
    private readonly ITravelItemRepository _repository;

    public SaveService(
        ITravelItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<SaveResult> SaveAsync(
        SaveTravelItemRequest request, bool skipDuplicateCheck = false)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return new SaveResult
            {
                Status = SaveResultStatus.ValidationError,
                Message = "URL 不可以是空的。"
            };
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return new SaveResult
            {
                Status = SaveResultStatus.ValidationError,
                Message = "分類不可以是空的。"
            };
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new SaveResult
            {
                Status = SaveResultStatus.ValidationError,
                Message = "名稱不可以是空的。"
            };
        }

        if (!skipDuplicateCheck)
        {
            var existingItem =
                await _repository.FindByNameAsync(request.Name);

            if (existingItem != null)
            {
                return new SaveResult
                {
                    Status = SaveResultStatus.Duplicate,
                    Message = $"名稱「{request.Name}」已經存在。"
                };
            }
        }

        var item = new TravelItem
        {
            Name = request.Name,
            Url = request.Url,
            Category = request.Category,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdItem =
            await _repository.CreateAsync(item);

        return new SaveResult
        {
            Status = SaveResultStatus.Success,
            Message = "旅遊資訊儲存成功。",
            ItemId = createdItem.Id
        };
    }
}