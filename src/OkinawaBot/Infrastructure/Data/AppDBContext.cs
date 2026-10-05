using Microsoft.EntityFrameworkCore;
using OkinawaBot.Domain.Entities;

namespace OkinawaBot.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TravelItem> TravelItems { get; set; }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TravelItem>()
            .ToTable("travel_items");
    }
}