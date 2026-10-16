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
        modelBuilder.Entity<TravelItem>(entity =>
        {
            entity.ToTable("travel_items");
            entity.Property(e => e.Destination)
                .HasDefaultValue("沖繩");
            entity.HasIndex(e => new { e.Destination, e.Category });
            entity.HasIndex(e => new { e.Destination, e.Name });
        });
    }
}