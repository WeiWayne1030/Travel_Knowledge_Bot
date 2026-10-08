using OkinawaBot.Domain.Entities;
using OkinawaBot.Domain.Interfaces;

namespace OkinawaBot.Tests.Fakes;

public class FakeTravelItemRepository : ITravelItemRepository
{
    private readonly List<TravelItem> _items = [];

    public IReadOnlyList<TravelItem> Items => _items;

    private int _nextId = 1;

    public Task<TravelItem?> FindByIdAsync(int id)
    {
        var item = _items.FirstOrDefault(x => x.Id == id);

        return Task.FromResult(item);
    }

    public Task<TravelItem?> FindByNameAsync(string name)
    {
        var item = _items.FirstOrDefault(
            x => x.Name == name);

        return Task.FromResult(item);
    }

    public Task<IReadOnlyList<TravelItem>>
        FindByCategoryAsync(string category)
    {
        IReadOnlyList<TravelItem> items =
            _items
                .Where(x => x.Category == category)
                .ToList();

        return Task.FromResult(items);
    }

    public Task<TravelItem> CreateAsync(TravelItem item)
    {
        item.Id = _nextId++;

        _items.Add(item);

        return Task.FromResult(item);
    }

    public Task UpdateAsync(TravelItem item)
    {
        var index = _items.FindIndex(
            x => x.Id == item.Id);

        if (index >= 0)
        {
            _items[index] = item;
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(TravelItem item)
    {
        _items.Remove(item);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TravelItem>> GetAllAsync()
    {
        IReadOnlyList<TravelItem> items =
            _items
                .OrderBy(x => x.Id)
                .ToList();

        return Task.FromResult(items);
    }

    public Task<IReadOnlyList<string>> GetDistinctCategoriesAsync()
    {
        IReadOnlyList<string> categories =
            _items
                .Select(x => x.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

        return Task.FromResult(categories);
    }
}