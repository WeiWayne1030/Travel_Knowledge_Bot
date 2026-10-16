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

    public Task<TravelItem?> FindByNameAsync(string name, string? destination = null)
    {
        var query = _items.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(destination))
        {
            query = query.Where(x => x.Destination == destination);
        }

        var item = query.FirstOrDefault(
            x => x.Name == name);

        return Task.FromResult(item);
    }

    public Task<IReadOnlyList<TravelItem>>
        FindByCategoryAsync(string category, string? destination = null)
    {
        var query = _items.Where(x => x.Category == category);

        if (!string.IsNullOrWhiteSpace(destination))
        {
            query = query.Where(x => x.Destination == destination);
        }

        IReadOnlyList<TravelItem> items = query.ToList();

        return Task.FromResult(items);
    }

    public Task<TravelItem> CreateAsync(TravelItem item)
    {
        item.Id = _nextId++;

        if (string.IsNullOrWhiteSpace(item.Destination))
        {
            item.Destination = "沖繩";
        }

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

    public Task<IReadOnlyList<TravelItem>> GetAllAsync(string? destination = null)
    {
        var query = _items.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(destination))
        {
            query = query.Where(x => x.Destination == destination);
        }

        IReadOnlyList<TravelItem> items =
            query
                .OrderBy(x => x.Id)
                .ToList();

        return Task.FromResult(items);
    }

    public Task<IReadOnlyList<string>> GetDistinctCategoriesAsync(string? destination = null)
    {
        var query = _items.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(destination))
        {
            query = query.Where(x => x.Destination == destination);
        }

        IReadOnlyList<string> categories =
            query
                .Select(x => x.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

        return Task.FromResult(categories);
    }

    public Task<IReadOnlyList<string>> GetDistinctDestinationsAsync()
    {
        IReadOnlyList<string> destinations =
            _items
                .Select(x => x.Destination)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

        return Task.FromResult(destinations);
    }
}