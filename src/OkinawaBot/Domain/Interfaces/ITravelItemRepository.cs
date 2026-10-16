using OkinawaBot.Domain.Entities;

namespace OkinawaBot.Domain.Interfaces;

public interface ITravelItemRepository
{
    Task<TravelItem?> FindByIdAsync(int id);

    Task<TravelItem?> FindByNameAsync(string name, string? destination = null);

    Task<IReadOnlyList<TravelItem>> FindByCategoryAsync(
        string category, string? destination = null);

    Task<TravelItem> CreateAsync(
        TravelItem item);
    Task UpdateAsync(
        TravelItem item);

    Task DeleteAsync(
        TravelItem item);

    Task<IReadOnlyList<TravelItem>> GetAllAsync(string? destination = null);

    Task<IReadOnlyList<string>> GetDistinctCategoriesAsync(string? destination = null);

    Task<IReadOnlyList<string>> GetDistinctDestinationsAsync();
}