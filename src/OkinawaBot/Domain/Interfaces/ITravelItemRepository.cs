using OkinawaBot.Domain.Entities;

namespace OkinawaBot.Domain.Interfaces;

public interface ITravelItemRepository
{
    Task<TravelItem?> FindByIdAsync(int id);

    Task<TravelItem?> FindByNameAsync(string name);

    Task<IReadOnlyList<TravelItem>> FindByCategoryAsync(
        string category);

    Task<TravelItem> CreateAsync(
        TravelItem item);
    Task UpdateAsync(
        TravelItem item);

    //Task DeleteAsync(
    //    TravelItem item);

    Task<IReadOnlyList<TravelItem>> GetAllAsync();
}