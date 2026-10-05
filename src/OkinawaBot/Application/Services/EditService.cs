using OkinawaBot.Application.Models;
using OkinawaBot.Domain.Entities;
using OkinawaBot.Domain.Interfaces;

namespace OkinawaBot.Application.Services;

public class EditService
{
    private readonly ITravelItemRepository _repository;

    public EditService(
        ITravelItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<EditResult> UpdateAsync(
        int itemId,
        UpdateTravelItemRequest request,
        bool skipDuplicateCheck = false)
    {
        var item =
            await _repository.FindByIdAsync(itemId);

        if (item is null)
        {
            return EditResult.NotFound();
        }

        if (!skipDuplicateCheck)
        {
            var existingItem =
                await _repository.FindByNameAsync(
                    request.Name);

            if (existingItem is not null &&
                existingItem.Id != itemId)
            {
                return EditResult.Duplicate();
            }
        }

        item.Url = request.Url;
        item.Category = request.Category;
        item.Name = request.Name;

        await _repository.UpdateAsync(item);

        return EditResult.Success(item);
    }

    public async Task<IReadOnlyList<TravelItem>>
    GetEditableItemsAsync()
    {
        return await _repository.GetAllAsync();
    }
}
