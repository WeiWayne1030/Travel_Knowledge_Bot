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

    [Fact]
    public async Task EditFlow_ShouldUpdateTravelItem()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = "integration-edit-user";
        int itemId;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var item = new OkinawaBot.Domain.Entities.TravelItem
            {
                Name = "舊的景點",
                Url = "https://example.com/old",
                Category = "OldCat"
            };
            db.TravelItems.Add(item);
            await db.SaveChangesAsync();
            itemId = item.Id;
        }

        // Act 1: 使用者輸入 Edit 進入編輯模式
        var editCommandRequest = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
                {
                    Type = "message",
                    Source = new LineSource { UserId = userId },
                    Message = new LineMessage { Type = "text", Text = "Edit" }
                }
            ]
        };

        var firstResponse = await client.PostAsJsonAsync("/api/LineWebhook", editCommandRequest);
        Assert.True(firstResponse.IsSuccessStatusCode);

        // 動態取得在資料庫中該筆資料的編號 (因為前面的測試可能留下了資料)
        int selectedIndex = 1;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var allItems = await db.TravelItems.ToListAsync();
            selectedIndex = allItems.FindIndex(x => x.Id == itemId) + 1;
        }

        // Act 2: 使用者輸入正確的編號選擇該筆資料
        var selectItemRequest = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
                {
                    Type = "message",
                    Source = new LineSource { UserId = userId },
                    Message = new LineMessage { Type = "text", Text = selectedIndex.ToString() }
                }
            ]
        };

        var secondResponse = await client.PostAsJsonAsync("/api/LineWebhook", selectItemRequest);
        Assert.True(secondResponse.IsSuccessStatusCode);

        var uniqueName = "新的景點_" + Guid.NewGuid().ToString();

        // Act 3: 使用者輸入新資料
        var updateDataRequest = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
                {
                    Type = "message",
                    Source = new LineSource { UserId = userId },
                    Message = new LineMessage { Type = "text", Text = $"https://example.com/new #NewCat {uniqueName}" }
                }
            ]
        };

        var thirdResponse = await client.PostAsJsonAsync("/api/LineWebhook", updateDataRequest);
        Assert.True(thirdResponse.IsSuccessStatusCode);
        
        // Assert: 確認回傳內容
        var responseBody = await thirdResponse.Content.ReadAsStringAsync();
        Assert.Contains("修改成功", responseBody);
        
        // Assert: 確認 Database 資料已被更新
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updatedItem = await db.TravelItems.FindAsync(itemId);
            
            Assert.NotNull(updatedItem);
            Assert.Equal(uniqueName, updatedItem.Name);
            Assert.Equal("https://example.com/new", updatedItem.Url);
            Assert.Equal("NewCat", updatedItem.Category);
        }
    }
}