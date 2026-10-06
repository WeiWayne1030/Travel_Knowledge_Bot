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

//Application Flow
builder.Services.AddScoped<SaveFlowHandler>();
builder.Services.AddScoped<QueryFlowHandler>();
builder.Services.AddScoped<EditFlowHandler>();
builder.Services.AddScoped<DeleteFlowHandler>();

//Application Utilities
builder.Services.AddScoped<BotCommandParser>();
builder.Services.AddScoped<SaveInputParser>();

//利用AddSingleton取代AddScoped,讓所有 HTTP Request 共用同一個 Dictionary => 暫解, 之後用Redis做控制
builder.Services.AddSingleton<ConversationStateManager>();

//Handlers
builder.Services.AddScoped<MessageHandler>();

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