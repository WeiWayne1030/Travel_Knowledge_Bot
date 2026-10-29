using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OkinawaBot.Infrastructure.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var userId = context.Items["UserId"]?.ToString() ?? "Unknown";
            var state = context.Items["ConversationState"]?.ToString() ?? "Unknown";

            _logger.LogError(
                ex,
                "Unhandled exception occurred. RequestPath: {RequestPath}, HttpMethod: {HttpMethod}, UserId: {UserId}, ConversationState: {ConversationState}",
                context.Request.Path.Value,
                context.Request.Method,
                userId,
                state);

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    error = "系統發生未預期的錯誤，請稍後再試。",
                    traceId = context.TraceIdentifier
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
            }
        }
    }
}
