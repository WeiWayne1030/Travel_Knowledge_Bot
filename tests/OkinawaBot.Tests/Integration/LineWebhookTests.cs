using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OkinawaBot.Models;

namespace OkinawaBot.Tests.Integration;

public class LineWebhookTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LineWebhookTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SaveCommand_ShouldReturnOk()
    {
        //模擬swagger的請求
        var client = _factory.CreateClient();

        var userId = "integration-test-user";

        // Act 1: 使用者輸入 Save
        var request = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
                {
                    Type = "message",
                Source = new LineSource
                {
                    UserId = userId
                },
                Message = new LineMessage
                {
                    Type = "text",
                    Text = "Save"
                }
                }
            ]
        };

        // Act 2: 使用者輸入旅遊資料
        var saveDataRequest = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
            {
                Type = "message",
                Source = new LineSource
                {
                    UserId = userId
                },
                Message = new LineMessage
                {
                    Type = "text",
                    Text = """
                           https://example.com/okinawa
                           #Attraction
                           美麗海水族館
                           """
                }
            }
            ]
        };

        var secondResponse =
        await client.PostAsJsonAsync(
            "/api/LineWebhook",
            saveDataRequest);

        // Assert 2
        Assert.Equal(
            HttpStatusCode.OK,
            secondResponse.StatusCode);
    }
}