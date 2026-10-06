using OkinawaBot.Domain.Entities;
using OkinawaBot.Domain.Interfaces;

namespace OkinawaBot.Application.Services;

public class DeleteService
{
    private readonly ITravelItemRepository _repository;

    public DeleteService(
        ITravelItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<TravelItem?> GetItemAsync(int itemId)
    {
        return await _repository.FindByIdAsync(itemId);
    }

    public async Task<bool> DeleteAsync(int itemId)
    {
        var item =
            await _repository.FindByIdAsync(itemId);

        if (item is null)
        {
            return false;
        }

        await _repository.DeleteAsync(item);

        return true;
    }

    public async Task<IReadOnlyList<TravelItem>>
        GetDeletableItemsAsync()
    {
        return await _repository.GetAllAsync();
    }
}