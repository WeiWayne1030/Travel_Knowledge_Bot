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

        // Wake up bot
        var wakeRequest = new LineWebhookRequest
        {
            Events = [ new LineEvent { Type = "message", ReplyToken = "token_wake", Source = new LineSource { UserId = userId }, Message = new LineMessage { Type = "text", Text = "旅遊小幫手" } } ]
        };
        await PostWebhookAsync(client, wakeRequest);

        // Act 1：使用者輸入 Save
        var saveCommandRequest = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
                {
                    Type = "message", ReplyToken = "token_1", Source = new LineSource
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
            await PostWebhookAsync(client, saveCommandRequest);

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
                    Type = "message", ReplyToken = "token_2", Source = new LineSource
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
            await PostWebhookAsync(client, saveDataRequest);

        // Assert 2：確認 Save API 成功
        

        Assert.True(
            secondResponse.IsSuccessStatusCode,
            $"StatusCode: {secondResponse.StatusCode}\n" +
            $"Response: empty");

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

        // Wake up bot
        var wakeRequest = new LineWebhookRequest
        {
            Events = [ new LineEvent { Type = "message", ReplyToken = "token_wake2", Source = new LineSource { UserId = userId }, Message = new LineMessage { Type = "text", Text = "旅遊小幫手" } } ]
        };
        await PostWebhookAsync(client, wakeRequest);

        // Act 1: 使用者輸入 Query 進入查詢模式
        var queryCommandRequest = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
                {
                    Type = "message", ReplyToken = "token_3", Source = new LineSource { UserId = userId },
                    Message = new LineMessage { Type = "text", Text = "Query" }
                }
            ]
        };

        var firstResponse = await PostWebhookAsync(client, queryCommandRequest);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Act 2: 使用者輸入欲查詢的類別
        var queryDataRequest = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
                {
                    Type = "message", ReplyToken = "token_4", Source = new LineSource { UserId = userId },
                    Message = new LineMessage { Type = "text", Text = "Shopping" }
                }
            ]
        };

        var secondResponse = await PostWebhookAsync(client, queryDataRequest);
        
        // Assert: 確認 HTTP 狀態與回傳內容
        Assert.True(secondResponse.IsSuccessStatusCode);
        
        
        
        // 確認回傳內容中包含我們寫入的資料
                var fakeClient = _factory.Services.GetRequiredService<OkinawaBot.Infrastructure.Line.ILineClient>() as OkinawaBot.Tests.Fakes.FakeLineClient;
        Assert.True(fakeClient.SentMessages.TryGetValue("token_4", out var responseBody));
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

        // Wake up bot
        var wakeRequest = new LineWebhookRequest
        {
            Events = [ new LineEvent { Type = "message", ReplyToken = "token_wake3", Source = new LineSource { UserId = userId }, Message = new LineMessage { Type = "text", Text = "旅遊小幫手" } } ]
        };
        await PostWebhookAsync(client, wakeRequest);

        // Act 1: 使用者輸入 Edit 進入編輯模式
        var editCommandRequest = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
                {
                    Type = "message", ReplyToken = "token_5", Source = new LineSource { UserId = userId },
                    Message = new LineMessage { Type = "text", Text = "Edit" }
                }
            ]
        };

        var firstResponse = await PostWebhookAsync(client, editCommandRequest);
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
                    Type = "message", ReplyToken = "token_6", Source = new LineSource { UserId = userId },
                    Message = new LineMessage { Type = "text", Text = selectedIndex.ToString() }
                }
            ]
        };

        var secondResponse = await PostWebhookAsync(client, selectItemRequest);
        Assert.True(secondResponse.IsSuccessStatusCode);

        var uniqueName = "新的景點_" + Guid.NewGuid().ToString();

        // Act 3: 使用者輸入新資料
        var updateDataRequest = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
                {
                    Type = "message", ReplyToken = "token_7", Source = new LineSource { UserId = userId },
                    Message = new LineMessage { Type = "text", Text = $"https://example.com/new #NewCat {uniqueName}" }
                }
            ]
        };

        var thirdResponse = await PostWebhookAsync(client, updateDataRequest);
        Assert.True(thirdResponse.IsSuccessStatusCode);
        
        // Assert: 確認回傳內容
        
                var fakeClient = _factory.Services.GetRequiredService<OkinawaBot.Infrastructure.Line.ILineClient>() as OkinawaBot.Tests.Fakes.FakeLineClient;
        Assert.True(fakeClient.SentMessages.TryGetValue("token_7", out var responseBody));
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

    [Fact]
    public async Task DeleteFlow_ShouldDeleteTravelItem()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = "integration-delete-user";
        int itemId;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var item = new OkinawaBot.Domain.Entities.TravelItem
            {
                Name = "要被刪除的景點_" + Guid.NewGuid().ToString(),
                Url = "https://example.com/delete-me",
                Category = "DeleteCat"
            };
            db.TravelItems.Add(item);
            await db.SaveChangesAsync();
            itemId = item.Id;
        }

        // Wake up bot
        var wakeRequest = new LineWebhookRequest
        {
            Events = [ new LineEvent { Type = "message", ReplyToken = "token_wake4", Source = new LineSource { UserId = userId }, Message = new LineMessage { Type = "text", Text = "旅遊小幫手" } } ]
        };
        await PostWebhookAsync(client, wakeRequest);

        // Act 1: 使用者輸入 Delete 進入刪除模式
        var deleteCommandRequest = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
                {
                    Type = "message", ReplyToken = "token_8", Source = new LineSource { UserId = userId },
                    Message = new LineMessage { Type = "text", Text = "Delete" }
                }
            ]
        };

        var firstResponse = await PostWebhookAsync(client, deleteCommandRequest);
        Assert.True(firstResponse.IsSuccessStatusCode);

        // 動態取得在資料庫中該筆資料的編號
        int selectedIndex = 1;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var allItems = await db.TravelItems.ToListAsync();
            selectedIndex = allItems.FindIndex(x => x.Id == itemId) + 1;
        }

        // Act 2: 使用者輸入編號選擇資料
        var selectItemRequest = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
                {
                    Type = "message", ReplyToken = "token_9", Source = new LineSource { UserId = userId },
                    Message = new LineMessage { Type = "text", Text = selectedIndex.ToString() }
                }
            ]
        };

        var secondResponse = await PostWebhookAsync(client, selectItemRequest);
        Assert.True(secondResponse.IsSuccessStatusCode);

        // Act 3: 使用者輸入 Continue 確認刪除
        var confirmRequest = new LineWebhookRequest
        {
            Events =
            [
                new LineEvent
                {
                    Type = "message", ReplyToken = "token_10", Source = new LineSource { UserId = userId },
                    Message = new LineMessage { Type = "text", Text = "Continue" }
                }
            ]
        };

        var thirdResponse = await PostWebhookAsync(client, confirmRequest);
        Assert.True(thirdResponse.IsSuccessStatusCode);
        
        // Assert: 確認回傳內容
        
                var fakeClient = _factory.Services.GetRequiredService<OkinawaBot.Infrastructure.Line.ILineClient>() as OkinawaBot.Tests.Fakes.FakeLineClient;
        Assert.True(fakeClient.SentMessages.TryGetValue("token_10", out var responseBody));
        Assert.Contains("刪除成功", responseBody);
        
        // Assert: 確認 Database 資料確實已被刪除
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var deletedItem = await db.TravelItems.FindAsync(itemId);
            
            Assert.Null(deletedItem);
        }
    }
    //把我們在測試設定的假密鑰 ("test_secret") 拿來，動態對假 Payload 進行 HMAC-SHA256 雜湊，並把產生的 Signature 塞入 Headers 裡，完美模擬真正的 LINE 伺服器行為。
    private async System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> PostWebhookAsync(System.Net.Http.HttpClient client, OkinawaBot.Models.LineWebhookRequest request)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
        var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var secretBytes = System.Text.Encoding.UTF8.GetBytes("test_secret");
        using var hmac = new System.Security.Cryptography.HMACSHA256(secretBytes);
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(json));
        var signature = Convert.ToBase64String(hash);
        client.DefaultRequestHeaders.Remove("x-line-signature");
        client.DefaultRequestHeaders.Add("x-line-signature", signature);
        return await client.PostAsync("/api/LineWebhook", content);
    }
}