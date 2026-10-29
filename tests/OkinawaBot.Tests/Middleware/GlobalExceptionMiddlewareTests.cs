using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OkinawaBot.Infrastructure.Middleware;

namespace OkinawaBot.Tests.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private class TestLogger<T> : ILogger<T>
    {
        public List<string> LoggedMessages { get; } = new();
        public List<Exception?> LoggedExceptions { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            LoggedMessages.Add(formatter(state, exception));
            LoggedExceptions.Add(exception);
        }
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
    {
        var logger = new TestLogger<GlobalExceptionMiddleware>();
        var calledNext = false;
        var middleware = new GlobalExceptionMiddleware(_ =>
        {
            calledNext = true;
            return Task.CompletedTask;
        }, logger);

        var context = new DefaultHttpContext();
        await middleware.InvokeAsync(context);

        Assert.True(calledNext);
        Assert.Empty(logger.LoggedMessages);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ShouldLogWithUserIdAndState_AndReturn500()
    {
        var logger = new TestLogger<GlobalExceptionMiddleware>();
        var expectedException = new InvalidOperationException("Something blew up!");
        var middleware = new GlobalExceptionMiddleware(_ => throw expectedException, logger);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/LineWebhook";
        context.Request.Method = "POST";
        context.Items["UserId"] = "user-123";
        context.Items["ConversationState"] = "SaveFlow";

        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        responseBody.Seek(0, SeekOrigin.Begin);
        var bodyText = new StreamReader(responseBody).ReadToEnd();
        Assert.Contains("系統發生未預期的錯誤", bodyText);

        var loggedMsg = Assert.Single(logger.LoggedMessages);
        Assert.Contains("user-123", loggedMsg);
        Assert.Contains("SaveFlow", loggedMsg);
        Assert.Contains("/api/LineWebhook", loggedMsg);
        Assert.Same(expectedException, logger.LoggedExceptions[0]);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrownWithoutItems_ShouldFallbackToUnknown()
    {
        var logger = new TestLogger<GlobalExceptionMiddleware>();
        var middleware = new GlobalExceptionMiddleware(_ => throw new Exception("Error"), logger);

        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        var loggedMsg = Assert.Single(logger.LoggedMessages);
        Assert.Contains("Unknown", loggedMsg);
    }
}
