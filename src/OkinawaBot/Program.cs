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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

builder.Services.AddScoped<ITravelItemRepository, TravelItemRepository>();

builder.Services.AddScoped<MessageHandler>();

builder.Services.AddScoped<SaveFlowHandler>();

builder.Services.AddScoped<SaveService>();

builder.Services.AddScoped<SaveInputParser>();

builder.Services.AddScoped<BotCommandParser>();

// 使用 AddSingleton 讓所有 HTTP Request 共用同一個 ConversationStateManager 實例
builder.Services.AddSingleton<ConversationStateManager>();

var app = builder.Build();

app.MapControllers();

app.Run();