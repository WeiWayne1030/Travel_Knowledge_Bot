using Microsoft.EntityFrameworkCore;
using OkinawaBot.Domain.Entities;
using OkinawaBot.Domain.Interfaces;
using OkinawaBot.Infrastructure.Data;

namespace OkinawaBot.Infrastructure.Repositories;

public class TravelItemRepository : ITravelItemRepository
{
    private readonly AppDbContext _db;

    public TravelItemRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TravelItem?> FindByIdAsync(int id)
    {
        return await _db.TravelItems
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<TravelItem?> FindByNameAsync(
        string name, string? destination = null)
    {
        var query = _db.TravelItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(destination))
        {
            query = query.Where(x => x.Destination == destination);
        }

        return await query
            .FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<IReadOnlyList<TravelItem>>
        FindByCategoryAsync(string category, string? destination = null)
    {
        var query = _db.TravelItems.Where(x => x.Category == category);

        if (!string.IsNullOrWhiteSpace(destination))
        {
            query = query.Where(x => x.Destination == destination);
        }

        return await query.ToListAsync();
    }

    public async Task<TravelItem> CreateAsync(
        TravelItem item)
    {
        _db.TravelItems.Add(item);

        await _db.SaveChangesAsync();

        return item;
    }

    public async Task UpdateAsync(
        TravelItem item)
    {
        _db.TravelItems.Update(item);

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(
        TravelItem item)
    {
        _db.TravelItems.Remove(item);

        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<TravelItem>> GetAllAsync(string? destination = null)
    {
        var query = _db.TravelItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(destination))
        {
            query = query.Where(x => x.Destination == destination);
        }

        return await query
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<string>> GetDistinctCategoriesAsync(string? destination = null)
    {
        var query = _db.TravelItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(destination))
        {
            query = query.Where(x => x.Destination == destination);
        }

        return await query
            .Select(x => x.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<string>> GetDistinctDestinationsAsync()
    {
        return await _db.TravelItems
            .Select(x => x.Destination)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }
}