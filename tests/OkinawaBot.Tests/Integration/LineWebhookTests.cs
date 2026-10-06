using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OkinawaBot.Infrastructure.Data;
using OkinawaBot.Models;

namespace OkinawaBot.Tests.Integration;

public class LineWebhookTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public LineWebhookTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SaveFlow_ShouldSaveTravelItem()
    {
        //模擬swagger的請求
        var client = _factory.CreateClient();

        var userId = "integration-test-user";

        // Act 1：使用者輸入 Save
        var saveCommandRequest = new LineWebhookRequest
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

        var firstResponse =
            await client.PostAsJsonAsync(
                "/api/LineWebhook",
                saveCommandRequest);

        // Assert 1：確認進入 Save Flow 成功
        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        // Act 2：使用者輸入真正的資料

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

        // Assert 2：確認 Save API 成功
        var responseBody =
            await secondResponse.Content.ReadAsStringAsync();

        Assert.True(
            secondResponse.IsSuccessStatusCode,
            $"StatusCode: {secondResponse.StatusCode}\n" +
            $"Response: {responseBody}");

        // Assert 3：確認 Database 真的有資料
        using var scope =
            _factory.Services.CreateScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        var item =
            await db.TravelItems
                .SingleAsync(
                    x => x.Name == "美麗海水族館");

        // Assert 4：確認資料內容正確
        Assert.Equal(
            "https://example.com/okinawa",
            item.Url);

        Assert.Equal(
            "Attraction",
            item.Category);

        Assert.Equal(
            "美麗海水族館",
            item.Name);
    }

    [Fact]
    public async Task QueryFlow_ShouldReturnQueriedItems()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = "integration-query-user";
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.TravelItems.Add(new OkinawaBot.Domain.Entities.TravelItem
            {
                Name = "美國村",
                Url = "https://example.com/american-village",
                Category = "Shopping"
            });
            await db.SaveChangesAsync();
        }

        // Act 1: 使用者輸入 Query 進入查詢模式
        var queryCommandRequest = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
                {
                    Type = "message",
                    Source = new LineSource { UserId = userId },
                    Message = new LineMessage { Type = "text", Text = "Query" }
                }
            ]
        };

        var firstResponse = await client.PostAsJsonAsync("/api/LineWebhook", queryCommandRequest);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Act 2: 使用者輸入欲查詢的類別
        var queryDataRequest = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
                {
                    Type = "message",
                    Source = new LineSource { UserId = userId },
                    Message = new LineMessage { Type = "text", Text = "Shopping" }
                }
            ]
        };

        var secondResponse = await client.PostAsJsonAsync("/api/LineWebhook", queryDataRequest);
        
        // Assert: 確認 HTTP 狀態與回傳內容
        Assert.True(secondResponse.IsSuccessStatusCode);
        
        var responseBody = await secondResponse.Content.ReadAsStringAsync();
        
        // 確認回傳內容中包含我們寫入的資料
        Assert.Contains("【Shopping】", responseBody);
        Assert.Contains("美國村", responseBody);
        Assert.Contains("https://example.com/american-village", responseBody);
    }
}