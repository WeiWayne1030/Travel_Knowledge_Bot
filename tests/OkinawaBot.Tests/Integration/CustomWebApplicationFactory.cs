using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OkinawaBot.Infrastructure.Data;
using OkinawaBot.Infrastructure.Line;
using System.Linq;

namespace OkinawaBot.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.Configure<LineBotOptions>(options => 
            {
                options.ChannelSecret = "test_secret";
                options.ChannelAccessToken = "";
            });

            // Remove real Redis Cache
            var redisDesc = services.SingleOrDefault(d => d.ServiceType == typeof(Microsoft.Extensions.Caching.Distributed.IDistributedCache));
            if (redisDesc != null) services.Remove(redisDesc);

            // Remove real LineClient
            var lineClientDesc = services.SingleOrDefault(d => d.ServiceType == typeof(ILineClient));
            if (lineClientDesc != null) services.Remove(lineClientDesc);

            // Add Fakes
            services.AddSingleton<Microsoft.Extensions.Caching.Distributed.IDistributedCache, OkinawaBot.Tests.Fakes.FakeDistributedCache>();
            services.AddSingleton<ILineClient, OkinawaBot.Tests.Fakes.FakeLineClient>();
        });
    }

    protected override Microsoft.Extensions.Hosting.IHost CreateHost(Microsoft.Extensions.Hosting.IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.Migrate();
        return host;
    }
}