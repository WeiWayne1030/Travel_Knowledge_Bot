using OkinawaBot.Domain.Entities;
using OkinawaBot.Domain.Interfaces;

namespace OkinawaBot.Application.Services;

public class QueryService
{
    private readonly ITravelItemRepository _repository;

    public QueryService(
        ITravelItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<TravelItem>>
        QueryByCategoryAsync(string category, string? destination = null)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return [];
        }

        return await _repository.FindByCategoryAsync(
            category, destination);
    }

    public async Task<IReadOnlyList<string>> GetAvailableCategoriesAsync(string? destination = null)
    {
        return await _repository.GetDistinctCategoriesAsync(destination);
    }

    public async Task<IReadOnlyList<string>> GetAvailableDestinationsAsync()
    {
        return await _repository.GetDistinctDestinationsAsync();
    }

    public async Task<IReadOnlyList<TravelItem>> GetAllItemsAsync(string? destination = null)
    {
        return await _repository.GetAllAsync(destination);
    }
}