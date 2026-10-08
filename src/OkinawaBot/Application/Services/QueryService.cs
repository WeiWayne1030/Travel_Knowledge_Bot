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
        QueryByCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return [];
        }

        return await _repository.FindByCategoryAsync(
            category);
    }

    public async Task<IReadOnlyList<string>> GetAvailableCategoriesAsync()
    {
        return await _repository.GetDistinctCategoriesAsync();
    }
}