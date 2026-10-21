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

// 使用 Redis 做 Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "OkinawaBot_";
});

// 使用 Scoped，因為每次 Request 處理完可以將狀態存回 Redis
builder.Services.AddScoped<ConversationStateManager>();

//新增controller
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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