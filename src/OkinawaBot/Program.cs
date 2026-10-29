using Microsoft.EntityFrameworkCore;
using OkinawaBot.Application.Commands;
using OkinawaBot.Application.Flows;
using OkinawaBot.Application.Handlers;
using OkinawaBot.Application.Input;
using OkinawaBot.Application.Services;
using OkinawaBot.Application.State;
using OkinawaBot.Domain.Interfaces;
using OkinawaBot.Infrastructure.Data;
using OkinawaBot.Infrastructure.Repositories;
using OkinawaBot.Infrastructure.Line;

var builder = WebApplication.CreateBuilder(args);

//Infrastructuer
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (builder.Environment.IsEnvironment("Testing") ||
        connectionString?.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) == true)
    {
        options.UseSqlite(builder.Environment.IsEnvironment("Testing")
            ? "Data Source=test.db"
            : connectionString);
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

//repository優先處理
builder.Services.AddScoped<ITravelItemRepository, TravelItemRepository>();

//Application Services
builder.Services.AddScoped<SaveService>();
builder.Services.AddScoped<QueryService>();
builder.Services.AddScoped<EditService>();
builder.Services.AddScoped<DeleteService>();

// LINE Messaging API
//綁定lineBotOptions=>config中的LineBot設定
builder.Services.Configure<LineBotOptions>(
    builder.Configuration.GetSection(LineBotOptions.SectionName));
//綁定LineSignatureValidator(防偽簽名)
builder.Services.AddSingleton<LineSignatureValidator>();
//使用HttpClientFactory管理HttpClient->LineClient(LineMessagingAPI)
builder.Services.AddHttpClient<ILineClient, LineClient>();

//MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

//Application Utilities
builder.Services.AddScoped<BotCommandParser>();
builder.Services.AddScoped<SaveInputParser>();
builder.Services.AddScoped<EditInputParser>();

// Distributed Cache (優先嘗試 Redis，若未設定或連線失敗則自動降級為記憶體快取)
var redisConn = builder.Configuration.GetConnectionString("RedisConnection");
bool useRedis = false;

if (!string.IsNullOrWhiteSpace(redisConn) &&
    !redisConn.Equals("none", StringComparison.OrdinalIgnoreCase) &&
    !redisConn.Equals("memory", StringComparison.OrdinalIgnoreCase) &&
    !builder.Environment.IsEnvironment("Testing"))
{
    try
    {
        var configOptions = StackExchange.Redis.ConfigurationOptions.Parse(redisConn);
        configOptions.ConnectTimeout = 1000;
        configOptions.SyncTimeout = 1000;
        configOptions.AbortOnConnectFail = true;
        using var multiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect(configOptions);
        if (multiplexer.IsConnected)
        {
            useRedis = true;
        }
    }
    catch
    {
        useRedis = false;
    }
}

if (useRedis)
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConn;
        options.InstanceName = "OkinawaBot_";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
    Console.WriteLine("[Cache] 未連線至 Redis，已自動啟用記憶體快取 (DistributedMemoryCache)。");
}

// 使用 Scoped，因為每次 Request 處理完可以將狀態存回 Redis
builder.Services.AddScoped<ConversationStateManager>();

// HttpContext & Controllers
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<OkinawaBot.Infrastructure.Middleware.GlobalExceptionMiddleware>();

// 自動套用資料庫 Migration (如果在 NorthFlank 這種 Container 環境，重啟時會確保資料庫檔案和資料表存在)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

public partial class Program
{
}