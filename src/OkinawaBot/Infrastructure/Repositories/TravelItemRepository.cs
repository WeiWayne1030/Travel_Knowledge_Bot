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
        string name)
    {
        return await _db.TravelItems
            .FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<IReadOnlyList<TravelItem>>
        FindByCategoryAsync(string category)
    {
        return await _db.TravelItems
            .Where(x => x.Category == category)
            .ToListAsync();
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

    public async Task<IReadOnlyList<TravelItem>> GetAllAsync()
    {
        return await _db.TravelItems
            .OrderBy(x => x.Id)
            .ToListAsync();
    }
}