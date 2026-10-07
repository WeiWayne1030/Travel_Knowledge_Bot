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
    options.UseSqlite(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

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