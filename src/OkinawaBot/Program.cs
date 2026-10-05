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

//Application Flow
builder.Services.AddScoped<SaveFlowHandler>();
builder.Services.AddScoped<QueryFlowHandler>();
builder.Services.AddScoped<EditFlowHandler>();

//Application Utilities
builder.Services.AddScoped<BotCommandParser>();
builder.Services.AddScoped<SaveInputParser>();

//利用AddSingleton取代AddScoped,讓所有 HTTP Request 共用同一個 Dictionary
builder.Services.AddSingleton<ConversationStateManager>();

//Handlers
builder.Services.AddScoped<MessageHandler>();

var app = builder.Build();

app.MapControllers();

app.Run();