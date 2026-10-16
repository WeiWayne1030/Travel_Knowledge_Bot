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

        if (!skipDuplicateCheck &&
            !string.IsNullOrWhiteSpace(request.Name) &&
            !string.Equals(request.Name, item.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existingItem =
                await _repository.FindByNameAsync(
                    request.Name, item.Destination);

            if (existingItem is not null &&
                existingItem.Id != itemId)
            {
                return EditResult.Duplicate();
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Destination))
        {
            item.Destination = request.Destination;
        }

        if (!string.IsNullOrWhiteSpace(request.Url))
        {
            item.Url = request.Url;
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            item.Category = request.Category;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            item.Name = request.Name;
        }

        await _repository.UpdateAsync(item);

        return EditResult.Success(item);
    }

    public async Task<IReadOnlyList<TravelItem>>
    GetEditableItemsAsync(string? destination = null)
    {
        return await _repository.GetAllAsync(destination);
    }
}
