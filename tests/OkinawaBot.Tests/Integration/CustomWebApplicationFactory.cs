using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OkinawaBot.Infrastructure.Data;

namespace OkinawaBot.Tests.Integration;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor =
                services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(
                options =>
                {
                    options.UseSqlite(
                        "Data Source=test.db");
                });

            // 建立測試 Database
            var serviceProvider = services.BuildServiceProvider();

            using var scope =
                serviceProvider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            //如果 Database / Schema 不存在，就建立它。
            db.Database.EnsureCreated();
        });
    }
}