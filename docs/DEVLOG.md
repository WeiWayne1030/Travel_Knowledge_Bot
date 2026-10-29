# 📓 開發歷程紀錄 (Development Log)

本文件記錄 **Okinawa Travel Knowledge Bot** 從需求分析到實作的開發歷程，方便日後回顧設計決策、追蹤進度與規劃下一步。

> 最後更新：2026-10-29
> 分支：`main`（遠端 `origin/main`）

---

## 🗺️ 時間軸總覽

| # | 日期 | Commit | 階段 | 摘要 |
|---|------|--------|------|------|
| 1 | 2026-09-30 13:59 | `0c46744` | 需求與系統設計 | 撰寫 SRS、系統設計文件，建立 .NET 9 Web API 專案骨架 |
| 2 | 2026-09-30 15:59 | `0e151c9` | 資料層建置 | 建立 Entity、Repository、DbContext、SQLite 與 EF Core Migration |
| 3 | 2026-09-30 16:09 | `e513690` | 文件 | 新增 README（環境準備、安裝、執行說明） |
| 4 | 2026-10-05 09:21 | `1c75a25` | Save 功能基礎實作 | 指令解析、狀態管理、Save Flow / Service / Input Parser，以及單元測試專案 |
| 5 | 2026-10-05 10:41 | `92c5e89` | Save 重複名稱確認 | 新增 `SaveDuplicateConfirmation` 狀態與 Continue / Return 確認流程 |
| 6 | 2026-10-05 10:46 | `bc07934` | SRS 與實作對齊 | Save 輸入格式定為 `URL #Category Name`；FR-016 改成先確認再儲存；新增 DEVLOG |
| 7 | 2026-10-05 10:56 | `66b3467` | 技術債修正 (訊息與語言) | `MessageHandler` 改為回傳 `Task<BotResponse>`；驗證訊息統一為繁體中文 |
| 8 | 2026-10-05 11:05 | `6f3d4de` | 技術債與命名清理 | 修正 `Infrastructure` 拼字、`ConversationState.cs` 檔名、`ConcurrentDictionary` 狀態管理、刪除 `test.cs` 及取消追蹤 `bin/obj` 產物 |
| 9 | 2026-10-06 | `e375d38` | 補齊所有流程與環境修復 | 完成 Query、Edit、Delete Flow，修復 MessageHandler 測試，並修正 Swagger 在 .NET 9 下的衝突與啟動設定 |
| 10 | 2026-10-06 | `64825ee` | 補齊所有流程的整合測試 | 在 `LineWebhookTests.cs` 中補上 Query、Edit、Delete Flow 的 Webhook 整合測試，處理測試間 DB 資料互相污染問題。目前 64 測試全數通過 |
| 11 | 2026-10-06 | `099a356` | 引入 MediatR 架構重構 | 為了解耦 Controller 與 Handler，引入 MediatR 套件，將所有 FlowHandler 重構為 `IRequestHandler`，並將主邏輯移至 `ProcessMessageCommandHandler`，全面修復單元測試。 |
| 12 | 2026-10-07 | `c175143` | LINE Webhook 串接與 API 控制器實作 | 實作 `LineWebhookController`、`LineSignatureValidator` 簽章驗證、`LineClient` API 回覆機制與完整 Webhook DTO 模型。 |
| 13 | 2026-10-07 | `a812092` | 整合 Redis 狀態管理與 Docker 部署 | 引入 StackExchangeRedisCache 進行對話狀態分散式快取，建立 Dockerfile 與 docker-compose.yml 達成 API、Redis 與 SQLite 資料持久化部署。 |
| 14 | 2026-10-07 | `b5329fc` | 徹底重構與資安修補 | 將測試用的後門與不雅程式碼移除。導入 `ILineClient` 介面進行 DI 抽換，並在測試環境動態生成真實 HMAC 簽章以修補 `LineSignatureValidator` 的資安漏洞。 |
| 15 | 2026-10-08 | `e375d38` | 流程優化與中文化體驗 | 新增喚醒/休眠機制（旅遊小幫手/再見小幫手）。全面將指令中文化（儲存、查詢、編輯、刪除、繼續、返回主選單）。強化 Query 功能動態拉取資料庫現有分類提示使用者。 |
| 16 | 2026-10-14 | `ebce0a3` | 修復 CRUD 流程無法退出選單 | 「再見小幫手」/「返回主選單」改為全域攔截；返回主選單會重置暫存並重新顯示喚醒提示；統一提示文字為「返回主選單」。 |
| 17 | 2026-10-14 | `3a2bf2e` | 編輯流程彈性化重構 (任一欄位修改) | 實作 `EditInputParser` 與 `EditInputParseResult`，支援 URL、類別、名稱單一或多項自由修改；`UpdateTravelItemRequest` 改為 Nullable；`EditService` 支援局部更新並優化重複名稱檢查。 |
| 18 | 2026-10-16 | `bc98fff` | 多目的地分區支援 (切換地區/國家) | TravelItem 增加 Destination 欄位並配置預設值與索引；ITravelItemRepository / Services 支援地區篩選；新增 SwitchDestination 狀態與「切換地區」指令。 |
| 19 | 2026-10-16 | — | Redis 快取延長與流程逾時分級管理 | 將 Redis 快取延長至 1 天並保留地區偏好，加入 1 小時暫態流程逾時重置與提示機制；修補 DI 容器漏註冊與 EditInputParser URL 解析 Bug，98 測試全數通過。 |
| 20 | 2026-10-21 | — | 技術債清理與品質強化 | 建立 GitHub Actions CI 自動化測試建置流程；移除 Application/Models 下重複的 LineWebhookRequest；為 SaveInputParser 補齊 URL http/https 格式驗證與單元測試。 |
| 21 | 2026-10-21 | — | 連續操作支援與待機逾時調整 (10分鐘) | 各流程完成後回到 MainMenu 實現免重複喚醒連續操作；將待機逾時由 1 小時調至 10 分鐘；100 項測試全數通過。 |
| 22 | 2026-10-21 | — | 補齊下一步提示與共用訊息提取 | 盤點並補齊各流程缺少的「返回主選單／再見小幫手」提示；重複文字提取至 `BotMessages`（`WithMainMenu`、`WithRetryHint` 等）；100 項測試全數通過。 |
| 23 | 2026-10-28 | — | Redis 自動探測與記憶體快取降級 | 啟動時加入 Redis 連線探測與逾時防護，若未設定或連線失敗自動降級使用 DistributedMemoryCache，避免本機無 Redis 時報錯。 |
| 24 | 2026-10-29 | — | LINE Quick Reply 按鈕化 | 各回覆依狀態附上 Quick Reply 按鈕（主選單、類別、編號、繼續／返回主選單、再見小幫手），使用者幾乎不用打字；106 項測試全數通過。 |
---

## 📌 各階段詳細紀錄

### Phase 1 — 需求分析與系統設計（`0c46744`）

**目標**：釐清問題，定義 MVP 範圍，規劃架構。

- **問題背景**：沖繩旅遊資訊都是把 URL 丟進 LINE 群組，沒有分類也沒有結構，連結一多就很難找、很難重複使用。
- **產出文件**
  - [SRS.md](./SRS.md)：軟體需求規格書，定義 FR-001 ~ FR-016，並以 FR / BR / AC 三層撰寫：
    - **FR** = 系統要做什麼
    - **BR** = 系統有哪些規則
    - **AC** = 怎樣才算完成（Given / When / Then）
  - [design.md](./design.md)：狀態流程圖、系統架構圖、Repository 目錄規劃、SQLite 資料表規劃。
- **重要設計決策**
  - MVP 只做 **Save / Query / Edit / Delete** 四個功能（BR-001）。
  - **分類完全由使用者以 `#Category` 指定**，第一版不使用 AI，也不依 URL 網域自動分類（FR-007、BR-010 ~ BR-012）。
  - 對話採 **State Machine** 設計，任何流程中都可以輸入 `Return` 回到主選單（FR-004）。
  - 分層架構：`Controller → MessageHandler → State Manager → Service → Repository → SQLite`。
- **程式碼**：用 .NET 9 Web API 範本建立 `OkinawaBot.sln` 與 `src/OkinawaBot` 專案，並加入 `.gitignore`。

### Phase 2 — 資料層與狀態列舉（`0e151c9`）

**目標**：讓資料可以被持久化。

- 新增 `global.json` 鎖定 .NET SDK 版本。
- **Domain**
  - `TravelItem` 實體：`Id`、`Name`、`Url`、`Category`、`CreatedAt`、`UpdatedAt`。
  - `ITravelItemRepository` 介面：`FindByIdAsync`、`FindByNameAsync`、`FindByCategoryAsync`、`CreateAsync`、`UpdateAsync`、`DeleteAsync`。
- **Infrastructure**
  - `AppDbContext`（EF Core + SQLite）。
  - `TravelItemRepository` 實作。
  - Migration `20260930071905_InitialCreate`，產生 `okinawa.db`。
- **Application**：`ConversationState` 列舉（MainMenu / SaveFlow / QueryFlow / EditFlow / DeleteFlow）。
- `Program.cs` 註冊 DbContext 與 Repository；`appsettings.json` 設定連線字串 `Data Source=okinawa.db`。

### Phase 3 — README（`e513690`）

- 新增 [README.md](../README.md)，內容包括環境需求、Clone / Restore / Migration / Run 的步驟和專案結構說明。

### Phase 4 — Save 功能基礎實作與測試（`1c75a25`）

**目標**：用 TDD 的方式，先把 Save 流程從頭到尾串起來。

**新增元件**

| 層級 | 檔案 | 職責 |
|------|------|------|
| Commands | `BotCommand` / `BotCommandParser` | 把文字（不分大小寫）解析成 Save / Query / Edit / Delete / Return / Unknown |
| Handlers | `MessageHandler` | 入口，依使用者目前狀態分派到對應的 Flow |
| Flows | `SaveFlowHandler` | Save 流程控制：處理 Return、解析輸入、呼叫 Service、切換狀態 |
| Input | `SaveInputParser` | 用 Regex 抓 `#Category`，前段當 URL、後段當 Name |
| Services | `SaveService` | 驗證必填欄位、檢查名稱重複、寫入 Repository |
| Services | `QueryService` / `EditService` / `DeleteService` | 空殼，先佔位 |
| States | `ConversationContext` / `ConversationStateManager` | 保存每位使用者的對話狀態 |
| Models | `BotResponse`、`SaveResult`、`SaveResultStatus`、`SaveTravelItemRequest`、`SaveInputParseResult` | 各層之間傳遞資料的 DTO |

**測試專案**：`tests/OkinawaBot.Tests`（xUnit + coverlet）

- `FakeTravelItemRepository`：記憶體版的假 Repository，讓測試不必連資料庫。
- 測試涵蓋：`SaveInputParser`、`SaveService`、`SaveFlowHandler`、`MessageHandler`、`ConversationStateManager`。

### Phase 5 — 重複名稱確認流程（`92c5e89`）

**目標**：處理 FR-016 名稱重複的情況。

- `ConversationState` 新增 `SaveDuplicateConfirmation`。
- `ConversationContext` 新增 `PendingSave`，暫存等待使用者確認的資料。
- `SaveService.SaveAsync` 新增 `skipDuplicateCheck` 參數。
- `SaveFlowHandler`：
  - 名稱重複時，暫存 request、切換到確認狀態，並提示「請輸入 Continue 或 Return」。
  - `Continue` → 略過重複檢查直接儲存，回到主選單。
  - `Return` → 取消儲存，清掉 `PendingSave`，回到主選單。
  - 其他輸入 → 再提示一次。
- `MessageHandler` 會把 `SaveDuplicateConfirmation` 狀態也轉給 `SaveFlowHandler`。

### Phase 6 — SRS 與實作對齊（`bc07934`）

**目標**：消除 SRS 和程式之間的差異。兩項都**以程式為準**，只修改 [SRS.md](./SRS.md)，不動程式碼。

- **Save 輸入格式（FR-003、FR-010）**
  - 格式從 `URL + name + #Category` 改成 **`URL #Category Name`**，單行或多行都可以。
  - 新增 **BR-030**：`#Category` 前面的文字是 URL，後面的文字是 Name，和 `SaveInputParser` 的行為一致。
- **名稱重複處理（FR-016）**
  - 標題從 *Duplicate Name Warning* 改成 **Duplicate Name Confirmation**。
  - BR-027 改成「使用者輸入 `Continue` 之後才允許重複名稱」。
  - 新增 **BR-031 ~ BR-033**：等待確認時暫存資料、輸入 `Return` 會取消並回主選單、其他輸入會再提示一次。
  - AC 擴充為 **AC-016-01 ~ AC-016-05**。

### Phase 7 — 技術債修正 (訊息與語言)（`66b3467`）

**目標**：修復部分已知技術債與統一訊息語言。

- `MessageHandler.HandleAsync` 改為回傳 `Task<BotResponse>`，讓 Flow 層的回覆訊息能正確向上傳遞。
- 主選單指令回應調整：進入 Save 時回覆格式說明（符合 AC-003-01/FR-001）；Query/Edit/Delete 回覆「尚未開放」並支援 `Return`。
- `SaveInputParser` 與 `SaveService` 的驗證與錯誤訊息統一改為繁體中文。

### Phase 8 — 技術債與命名清理（`6f3d4de`）

**目標**：修復資料夾拼字錯誤、增強執行階段安全性與清理無用檔案。

- **資料夾與檔名更正**：
  - 將資料夾 `Infastructure` 更名為 `Infrastructure`。
  - 將檔名 `ConversationSate.cs` 更名為 `ConversationState.cs`。
  - 將測試檔 `SaveServiceTests.cs` 更名為 `ConversationStateManagerTests.cs`。
- **狀態管理與資源控制**：
  - `ConversationStateManager` 的 `Dictionary` 改為 `ConcurrentDictionary` 確保 Thread-Safety。
  - 在 `Reset` 中將 `PendingSave` 一併設為 `null`，避免殘留過期暫存資料。
- **檔案與 Git 追蹤清理**：
  - 刪除根目錄無用範本 `test.cs`。
  - 修正 `Program.cs` 註解字元編碼。
  - 透過 `git rm --cached` 移除誤追蹤的 `bin/` 與 `obj/` 建置檔。

### Phase 9 — 補齊 Query、Edit、Delete 流程與修復 Swagger（`e375d38`）

**目標**：把 MVP 中剩下的三個主要對話流程做完，並確保單元測試與開發環境正常。

- **實作完整 Flow**：
  - `QueryFlowHandler`：能依照分類或是關鍵字查詢旅遊資訊。
  - `EditFlowHandler`：選擇要編輯的項目 -> 重新輸入新資料 -> 檢查重複名稱並進行確認 (`EditDuplicateConfirmation`)。
  - `DeleteFlowHandler`：選擇要刪除的項目 -> 進行二次確認 (`DeleteConfirmation`)。
- **整合 MessageHandler**：將上述 Handler 依賴全數注入，並修復 `HandleMainMenuAsync` 的 `Task` 回傳問題。現在從主選單輸入 Save、Query、Edit、Delete 都會回傳相對應的提示詞並切換狀態。
- **完善單元測試**：在 `MessageHandlerTests`、`EditFlowHandlerTests`、`DeleteFlowHandlerTests` 等補齊狀態切換和返回(`Return`)行為的測試，目前專案內 60 項單元測試全數通過。
- **修復 Swagger 開發環境**：
  - 在 .NET 9 中，原本加入的 `Microsoft.AspNetCore.OpenApi` 與 `Swashbuckle.AspNetCore` 有型別版本衝突，移除前者讓 Swashbuckle 正常運作。
  - 調整 `launchSettings.json`，於 `http`、`https` 與 `IIS Express` 配置補上 `"launchBrowser": true` 與 `"launchUrl": "swagger"`，使 `dotnet run` 能夠順利彈出測試介面。

### Phase 10 — 補齊所有流程的整合測試（`64825ee`）

**目標**：確保在實際 Web API (`/api/LineWebhook`) 收到各項 Flow 對應指令時，整個系統能正確回應，並且操作真正的 (In-Memory) 資料庫。

- **新增測試案例**：
  - `QueryFlow_ShouldReturnQueriedItems`：驗證輸入 Query 並指定分類後，能正確拉出在資料庫預設好的對應資料。
  - `EditFlow_ShouldUpdateTravelItem`：驗證能正確走完選定編號、輸入新資料並確保資料庫確實更新的流程。
  - `DeleteFlow_ShouldDeleteTravelItem`：驗證能順利選定編號、經過 Continue 刪除確認後，確保資料庫清空該筆紀錄。
- **排除整合測試陷阱 (Test Isolation)**：
  - 由於所有整合測試共用一個由 WebApplicationFactory 建置的 `test.db`，先前的測試插入了許多假資料並殘留在其中。如果固定傳送選單第一項 (`"1"`) 很容易選錯。
  - **解決方式**：測試碼改為用 EF Core 動態撈出剛才新增的資料在目前資料庫中的 `index`，送出該 `index` 確保操作對象正確。
  - **名稱防呆衝突**：Edit Flow 修改資料時，為了避免跟之前的測試殘留資料名稱重複而進到二次確認畫面，將測試用資料改以 `Guid` 產生隨機名稱，確保永遠可以走通 Happy Path。
- **成果**：專案涵蓋 64 項單元與整合測試，全部執行通過 (`Passed`)，為 LINE Webhook 的 Controller 接接鋪好了安全網。

### Phase 11 — 引入 MediatR 架構重構（`099a356`）

**目標**：為了解決未來擴充性問題以及 Controller 依賴過多 Handler 的情況，引入 MediatR 實踐 CQRS (Command Query Responsibility Segregation) 或 Mediator 設計模式。

- **新增套件**：透過 NuGet 安裝 `MediatR`。
- **重構 Command / Request**：
  - 將各個流程所需的輸入參數封裝成 Command DTO，例如：`SaveFlowCommand`、`QueryFlowCommand`、`EditFlowCommand`、`DeleteFlowCommand`，以及主流程的 `ProcessMessageCommand`。
- **重構 Handlers**：
  - 將原先的 `SaveFlowHandler`、`QueryFlowHandler` 等類別全數改為實作 `IRequestHandler<TCommand, BotResponse>`。
  - 將原本複雜且依賴多個 Handler 的 `MessageHandler` 刪除，新建 `ProcessMessageCommandHandler`。它只需注入 `ConversationStateManager` 與 `IMediator`，並透過 `_mediator.Send()` 來分派任務給各個子 Flow，達到完全解耦。
- **重構 Program.cs**：
  - 移除原先寫死的大量 `builder.Services.AddScoped<XxxFlowHandler>()`。
  - 改用 `builder.Services.AddMediatR(...)` 讓系統自動掃描並註冊所有 Handler。
- **重構單元與整合測試**：
  - 為測試專案引入 `Microsoft.Extensions.DependencyInjection` 與 `MediatR` 的 DI 環境。
  - 撰寫 PowerShell 自動化腳本，將 `tests/OkinawaBot.Tests/Flows/` 下高達數百行的測試檔內舊有 `.HandleAsync(userId, msg)` 呼叫，全面批量替換為 `.Handle(new OOOCommand(...), CancellationToken.None)`。
  - 修正 MediatR 在測試中缺少 `ILoggerFactory` 的啟動錯誤。
  - 最終 64 個測試全數 `Passed`，確保重構後系統行為與原先 100% 一致。

### Phase 12 — LINE Webhook 串接與 API 控制器實作（`c175143`）

**目標**：讓系統能真正接收來自 LINE Messaging API Webhook 的 HTTP 請求，驗證簽章真實性，並將訊息丟入 MediatR 處理後回傳給 LINE 使用者。

- **新增 LINE Infrastructure 元件**：
  - `LineBotOptions`：於 `appsettings.json` 設定 `ChannelSecret` 與 `ChannelAccessToken` 的綁定。
  - `LineSignatureValidator`：實作 HMAC-SHA256 簽章驗證，比對 Request Header 中的 `X-Line-Signature`，避免偽造請求。
  - `LineWebhookRequest`：建立包含 Event、Message 等完整的 LINE Webhook JSON 請求資料結構。
  - `LineClient`：建立獨立 HttpClient 服務，負責向 LINE API 發送 Reply Message。
- **實作 LineWebhookController**：
  - 開放 HTTP POST API endpoint `/api/LineWebhook`。
  - 於 Controller 內檢驗 `X-Line-Signature`，無效簽章即回傳 HTTP 401。
  - 解析事件，將文字訊息的 `UserId` 與 `Text` 封裝為 `ProcessMessageCommand` 透過 `IMediator` 分派處理。
  - 收到處理結果後，呼叫 `LineClient` 將回覆內容送回給 LINE 使用者，並透過 `ILogger` 記錄運行狀態。

### Phase 13 — 整合 Redis 狀態管理與 Docker 部署（`a812092`）

**目標**：為了解決未來容器化部署以及服務重啟導致對話狀態（`ConversationContext`）遺失的問題，將記憶體狀態管理（`ConcurrentDictionary`）遷移至分散式快取（Redis）。

- **修改架構**：
  - 安裝套件 `Microsoft.Extensions.Caching.StackExchangeRedis`。
  - 將 `ConversationStateManager` 的 DI 生命週期從 `Singleton` 變更為 `Scoped`。
  - 重構 `ConversationStateManager` 內部，使用 `IDistributedCache` 進行存取與 `System.Text.Json` 序列化。
  - 於 `ProcessMessageCommandHandler` 最末端新增 `await _stateManager.SaveCurrentAsync()`，無縫將狀態寫回 Redis。
- **建置 Docker 運行環境**：
  - 新增 `Dockerfile` 以供未來部署至 Zeabur 等容器服務平台。
  - 新增 `docker-compose.yml` 檔案，定義 `okinawabot` 與 `redis` 兩個服務。
  - 利用環境變數 `ConnectionStrings__RedisConnection=redis:6379` 覆寫原本 `appsettings.json` 的設定以符合 Docker 內部網路。
  - 配置 `./data` 掛載供 SQLite (`okinawa.db`) 持久化，並開啟 Redis 的 `appendonly` 以保存快取狀態。

### Phase 14 — 徹底重構與資安修補（`b5329fc`）

**目標**：清除所有為了整合測試而在正式程式碼中留下的不雅寫法（Code Smell）與資安後門。

- **重構 `LineWebhookController` 與 `LineClient`**：
  - 移除了原先為了測試攔截而塞入 `Response.Headers["X-Bot-Response"]` 的邏輯，讓 Controller 回歸純粹的 HTTP 處理。
  - 將 `LineClient` 抽離出 `ILineClient` 介面，並在 DI 容器中註冊。
  - 測試專案改為實作 `FakeLineClient` 並透過 `CustomWebApplicationFactory` 抽換，攔截 `ReplyToken` 驗證回覆結果。
- **修補 `LineSignatureValidator` 資安漏洞**：
  - 原先驗證器內包含 `if (signature == "dummy_signature_for_test") return true;` 的後門，這是極度危險的設計，會讓任何人輕易繞過驗證偽造 LINE 請求。
  - 移除該後門，並改在整合測試 `LineWebhookTests.cs` 中，利用測試用密鑰 `"test_secret"` 動態算出真實的 HMAC-SHA256 簽章並送出請求，以 100% 真實情境通過驗證。
- **結果**：正式環境的程式碼達到了 100% Clean Code，且 64 個整合/單元測試依然順利通過。

### Phase 15 — 流程優化與中文化體驗（`c6a97cd`）

**目標**：優化使用者體驗，讓機器人更貼近中文語境，並且增加進入選單的防呆機制。

- **新增喚醒與休眠機制**：
  - `ProcessMessageCommandHandler` 現在會攔截 `Idle` 狀態。使用者需輸入 `旅遊小幫手` 才能喚醒機器人並進入 `MainMenu`。
  - 在 `MainMenu` 時，輸入 `再見小幫手` 可讓系統重置為 `Idle` 狀態，避免意外觸發功能。
- **全面指令中文化**：
  - 原先的 `Save` / `Query` / `Edit` / `Delete` 變更為 `儲存` / `查詢` / `編輯` / `刪除`。
  - 二次確認與中斷操作的指令從 `Continue` / `Return` 變更為 `繼續` / `返回主選單`。
  - 修改 `BotCommandParser` 內部映射，並全數更新測試案例，確保 65 支單元/整合測試正確通過。
- **動態提示查詢分類**：
  - 修改 `ITravelItemRepository` 新增 `GetDistinctCategoriesAsync()`，可利用 EF Core 的 `.Distinct()` 語法，效能極佳地撈出資料庫現有分類。
  - 將 `QueryService` 注入至 `ProcessMessageCommandHandler`。當使用者輸入 `查詢` 時，除了顯示提示訊息外，會動態列出目前擁有的類別，大幅提升 UX（解決了 FR-008 未知分類的痛點）。

### Phase 16 — 修復 CRUD 流程無法退出選單（`ebce0a3`）

**問題**：喚醒進入主選單、再選擇 儲存／查詢／編輯／刪除 後，使用者無法退出選單。

- **原因分析**：
  - 主選單進入各流程的提示文字寫「返回選單」，但 `BotCommandParser` 只認得「返回主選單」，照著提示輸入會被判定為 `Unknown`。
  - 「再見小幫手」只在 `MainMenu` 狀態被處理，進入任何 Flow 後輸入皆無效。
- **修正內容**：
  - **全域指令攔截**：`ProcessMessageCommandHandler` 在非 `Idle` 狀態下，會先於分派至各 Flow 之前檢查：
    - `再見小幫手`（`BotCommand.Sleep`）→ `Reset` 回到 `Idle`，回覆「已退出小幫手。」
    - `返回主選單`（`BotCommand.Return`）→ `Reset` 清除 `CurrentItemId` / `PendingSave` / `PendingEdit` 後設為 `MainMenu`，並重新顯示喚醒提示。
  - **喚醒提示共用**：抽出常數 `WakeUpNotice`，喚醒與返回主選單共用同一段訊息。
  - **提示文字統一**：查詢／編輯／刪除的提示由「返回選單」改為「返回主選單」，與指令一致。
- **測試**：`MessageHandlerTests` 中兩個 Return 測試原本斷言為 `Idle`（與測試名稱不符），改為斷言 `MainMenu`。65 個測試全數通過。
- **後續**：各 FlowHandler 內的 `BotCommand.Return` 分支已被全域攔截，目前不會被執行，可視需要清除。

### Phase 17 — 編輯流程彈性化重構（支援單項/多項自由修改）（`3a2bf2e`）

**目標**：解除修改資料時強制必須輸入「URL、#類別、名稱」三項的限制，允許使用者自由修改單一欄位（如僅改 URL、僅改類別、僅改名稱）或任意組合，未輸入的欄位自動保留原值。

- **問題背景**：
  - 原先 `EditFlowHandler` 直接共用新增資料時所使用的 `SaveInputParser`。
  - `SaveInputParser` 嚴格要求 `URL #分類 名稱` 完整格式，造成使用者若只想更正名稱或換 URL，仍須把原本的三項資料重新輸入一遍，容易出錯且體驗不佳。
- **重構與設計決策**：
  - **專屬解析器 (`EditInputParser` & `EditInputParseResult`)**：
    - 建立專門針對編輯場景的輸入解析器，自 `SaveInputParser` 解耦。
    - **支援任意單一輸入**：
      - 僅改 URL：直接輸入 `https://...` 或 `http://...`
      - 僅改類別：直接輸入 `#新類別`
      - 僅改名稱：直接輸入新名稱字串
    - **支援任意組合輸入**：
      - `URL #類別`、`#類別 名稱`、`URL 名稱`、或全部三項（支援單行以空格分隔或多行輸入）。
    - **支援前綴標籤語法（選用）**：
      - 可使用 `名稱: xxx`、`類別: #xxx`（或 `分類: xxx`）、`網址: https://...`（或 `URL: ...`），提供更高的輸入明確度。
    - **輸入防呆與驗證**：
      - 檢查 URL 合法 Scheme（`http://` / `https://`）。
      - 阻擋僅輸入 `#` 或空白內容，並提供清楚的錯誤提示。
  - **模型調整 (`UpdateTravelItemRequest`)**：
    - 將屬性調整為 Nullable（`string? Url`、`string? Category`、`string? Name`），表彰局部更新語意。
  - **領域服務局部更新 (`EditService`)**：
    - `UpdateAsync` 改採局部更新模式：僅覆蓋非空值的欄位。
    - **重複名稱檢查優化**：只有當使用者有提供新名稱且與原始名稱不同時，才觸發 `FindByNameAsync` 檢查；若僅更新 URL 或類別，不會誤報重複名稱。
  - **流程提示優化 (`EditFlowHandler`)**：
    - 當使用者選擇項目編號時，提示訊息會列出該項目前完整的「名稱」、「類別」、「網址」，並附上單項與多項修改的操作說明。
  - **DI 註冊與測試覆蓋**：
    - 在 `Program.cs` 註冊 `EditInputParser`。
    - 新增 `EditInputParserTests` 驗證全欄位、單欄位（URL、類別、名稱）、雙欄位組合、標籤前綴與格式錯誤處理。
    - 更新 `EditServiceTests` 與 `EditFlowHandlerTests` 加入單一欄位更新與不變更名稱時跳過重複檢查的測試。

### Phase 18 — 多目的地分區支援（切換地區/國家）（`bc98fff`）

**目標**：在同一資料庫內支援不同國家或地區（例如：沖繩、東京、首爾、曼谷）的資料隔離，避免使用者前往不同旅遊地點時需建立新資料庫的困擾。

- **問題背景**：
  - 原先 `TravelItem` 實體與資料庫未記錄目的地資訊，預設全屬於沖繩。若日後儲存其他國家的景點與美食，在查詢分類時會將不同國家的項目混淆在一起。
- **重構與設計決策**：
  - **實體與資料庫層 (`TravelItem` & `AppDbContext`)**：
    - `TravelItem` 增加 `Destination` 欄位，預設值為 `"沖繩"`（既有舊資料自動相容為沖繩）。
    - 建立 EF Core Migration (`AddDestinationToTravelItem`)，針對 `(Destination, Category)` 與 `(Destination, Name)` 建立複合索引以加速查詢。
  - **資料存取層 (`ITravelItemRepository` & `TravelItemRepository`)**：
    - 各項查詢方法（`FindByNameAsync`、`FindByCategoryAsync`、`GetAllAsync`、`GetDistinctCategoriesAsync`）支援 `destination` 參數篩選。
    - 新增 `GetDistinctDestinationsAsync()` 查詢目前資料庫已存在的所有地區清單。
    - 命名重複性檢查改為依地區隔離（例如沖繩與東京可各自存在同名之「一蘭拉麵」）。
  - **對話狀態層 (`ConversationContext` & `ConversationState`)**：
    - `ConversationContext` 新增 `CurrentDestination` 屬性（預設為 `"沖繩"`），工作階段中保持記住使用者的選擇，直到主動切換。
    - 新增 `ConversationState.SwitchDestination` 狀態。
  - **流程與使用者體驗 (`ProcessMessageCommandHandler` & 各 FlowHandler)**：
    - 新增指令「`切換地區`」（亦支援「`切換目的地`」），可列出已儲存地區供號碼選擇，或直接打字輸入新地區名稱。
    - 主選單與喚醒提示明確標註目前地區（例：`目前地區：【沖繩】 📍`）。
    - 儲存時自動帶入當前地區，使用者每次貼網址時無須手打地區名稱。
    - 查詢、編輯、刪除自動限定於當前地區範圍，避免跨地區資料干擾。
  - **測試覆蓋**：
    - 更新 `FakeTravelItemRepository` 支援目的地篩選。
    - 於 `MessageHandlerTests`、`QueryServiceTests`、`EditFlowHandlerTests`、`DeleteFlowHandlerTests` 新增地區切換、依地區儲存/查詢/編輯/刪除之單元測試。

### Phase 19 — Redis 快取延長與流程逾時分級管理（2026-10-16）

**目標**：將 Redis Session 有效期延長至 1 天以維持使用者跨時段對話狀態（如目的地偏好），並實作流程逾時自動重置機制以避免長時間停滯造成的狀態陷阱（State Trap）。同時修補測試環境與輸入解析器缺陷，確保全數 98 個測試通過。

- **問題背景**：
  - 原先 Redis 快取（`IDistributedCache`）僅設定 1 小時滑動過期，對於通訊軟體的非同步使用習慣偏短，常導致使用者離開一陣子後設定好的目的地與狀態遺失。
  - 直接將快取延長至 1 天會衍生副作用：若使用者在表單/確認流程（如 `SaveFlow`、`EditFlow`、`DeleteFlow`）中途離開，隔天回來發送一般對話時會誤觸舊流程解析，導致狀態卡死或體驗困惑。
- **重構與設計決策**：
  - **狀態分級過期機制 (`ConversationContext` & `ConversationStateManager`)**：
    - `ConversationContext` 新增 `LastActiveAt`（最後活動時間戳記），於任何操作與狀態變更時自動刷新。
    - Redis 分散式快取設定為 1 天滑動過期（`SetSlidingExpiration(TimeSpan.FromDays(1))`），確保使用者基本偏好（`CurrentDestination`）長效保留。
    - `ConversationStateManager` 新增 `FlowTimeout`（預設 1 小時）與 `IsFlowExpired(context)` 判定方法。
  - **流程逾時攔截與友善恢復 (`ProcessMessageCommandHandler`)**：
    - 在處理任何訊息前，優先檢查非 `Idle` 的暫態流程是否逾時。
    - 若已逾時，自動將狀態重置（`Reset`，重置流程但保留使用者偏好目的地）：
      - 若使用者傳送一般文字，回覆提示：「`先前的操作已逾時並已重置，請輸入「旅遊小幫手」開始。`」。
      - 若使用者輸入「`旅遊小幫手`」，則直接重新喚醒進入主選單，並保留先前選定的地區。
  - **測試環境與解析器修復**：
    - **DI 容器漏註冊修復**：於 `MessageHandlerTests` 的 `ServiceCollection` 中補上缺漏的 `EditInputParser`，解決 MediatR 在執行 `EditFlowCommand` 時拋出的相依性注入錯誤。
    - **URL 協定正規表達式修復**：修正 `EditInputParser` 中的 `DirectUrlRegex`，改為匹配通用 URI scheme（`[a-zA-Z][a-zA-Z0-9+.-]*://\S+`），使無前綴的 `ftp://` 等非 http/https 連結能被正確辨識為 URL 候選並被 URL 格式檢驗擋下，修復 `EditInputParserTests.Parse_ShouldFail_WhenUrlIsInvalidScheme`。
  - **測試覆蓋**：
    - 於 `ConversationStateManagerTests` 增加 `IsFlowExpired` 涵蓋測試（Idle 狀態、有效時間內、逾時判定）。
    - 於 `MessageHandlerTests` 增加流程逾時自動重置提示與直接重新喚醒之整合測試。
    - 現有 98 項單元與整合測試全數通過（`Passed: 98, Failed: 0`）。

### Phase 20 — 技術債清理與品質強化（2026-10-21）

**目標**：清理專案中重複冗餘的類別模型、強化儲存流程的 URL 格式校驗，並導入 GitHub Actions CI 機制。

- **建立 CI/CD 自動化工作流程**：
  - 於 `.github/workflows/dotnet.yml` 建立標準 GitHub Actions CI 流程，在 `main` 分支的 push 與 PR 時自動執行 `dotnet restore`、`dotnet build` 與 `dotnet test`。
- **清除重複的 Webhook 模型**：
  - 廢棄 `Application/Models/LineWebhookRequest.cs`，整合測試（`LineWebhookTests.cs`）全數統一使用正式的 `OkinawaBot.Infrastructure.Line.LineWebhookRequest` 與 `LineWebhookEvent`。
- **完善 SaveFlow 的 URL 格式校驗與錯誤範例提示**：
  - 原先 `SaveInputParser` 僅檢查 URL 字串是否存在，未檢查 Scheme 與格式；現已補齊 `Uri.TryCreate` 與 http/https 協定檢查，與 `EditInputParser` 一致。
  - 各項輸入錯誤提示末端均附加標準格式範例（`URL #分類 名稱`），完善 FR-012。
  - 於 `SaveInputParseTests.cs` 與 `SaveFlowHandlerTests.cs` 補上對應單元測試。
- **編輯與刪除流程體驗優化 (UX)**：
  - 在主選單進入「編輯」與「刪除」流程時，動態調用 `QueryService.GetAllItemsAsync` 列出目前地區的所有旅遊項目編號、名稱與類別，使使用者不再需要盲猜編號。

### Phase 21 — 連續操作支援與待機逾時調整 (10分鐘)（2026-10-21）

**目標**：解決使用者完成單次操作後必須重新輸入「旅遊小幫手」的不便，實現連續操作體驗；同時將待機時間由 1 小時下調至 10 分鐘，兼顧使用流暢度與對話狀態安全性。

- **支援連續操作（Continuous Operation）**：
  - 於 `ConversationStateManager` 新增 `ResetToMainMenu(userId)` 方法，在清空單次操作暫存（`CurrentItemId`、`PendingSave`、`PendingEdit`）的同時，將狀態設為 `ConversationState.MainMenu`，並保留當前選定地區（`CurrentDestination`）。
  - 各業務流程（`SaveFlowHandler`、`QueryFlowHandler`、`EditFlowHandler`、`DeleteFlowHandler`）在成功執行或完成後，全面改呼叫 `ResetToMainMenu`，回覆訊息末尾統一附帶選單接續提示（`BotMessages.MainMenuPrompt`），使用者可直接輸入下一個操作或「再見小幫手」退出。
- **待機逾時時間微調為 10 分鐘**：
  - 將 `ConversationStateManager.FlowTimeout` 預設時間由 1 小時調整為 10 分鐘（`TimeSpan.FromMinutes(10)`）。
  - 使用者在主選單或各流程中若超過 10 分鐘未發送訊息，系統在被動接收下一則訊息時會自動判定過期並重置為 `Idle`，避免非同步通訊長期待機產生狀態陷阱。
- **測試與文件同步更新**：
  - 同步更新各 FlowHandler 單元測試中斷言狀態由 `Idle` 改為 `MainMenu`，目前全數 100 個測試全數通過（`Passed: 100, Failed: 0`）。
  - 同步更新 `SRS.md`（BR-035、FR-018）與 `design.md`（狀態轉移 Mermaid 圖與逾時說明）。

### Phase 22 — 補齊下一步提示與共用訊息提取（2026-10-21）

**目標**：使用者查詢完成後不知道下一步要做什麼。盤點所有流程後，補齊缺少的下一步提示，並把重複文字集中管理。

- **盤點結果**：共 12 個分支缺少下一步提示，分三類：
  - 子流程錯誤／防呆回覆沒有「返回主選單」選項（查詢、儲存、編輯、刪除、切換地區的空白、格式錯誤、無資料、編號錯誤或超出範圍）。
  - 進入儲存、編輯資料輸入時沒有說明可取消。
  - 完成與退出時沒有明確說明狀態：查詢成功沒有說「已回到主選單」，`再見小幫手` 沒有說明如何重新喚醒。
- **提示補齊**：
  - 查詢成功、儲存／編輯／刪除完成：附「已回到主選單。」與主選單提示。
  - 輸入錯誤留在原流程：附「請重新輸入，或輸入「返回主選單」。」。
  - 進入儲存、編輯資料輸入：附「（若要取消，請輸入「返回主選單」）」。
  - `再見小幫手`：「已退出小幫手。若要再次使用，請輸入「旅遊小幫手」。」
  - 無法處理的操作：「請輸入「返回主選單」或「旅遊小幫手」。」
- **共用訊息提取**：擴充 `BotMessages`，新增 `InvalidItemNumber`、`ItemNumberOutOfRange`、`ContinueOrReturnPrompt`、`SleepNotice`、`OperationNotSupported`、`WithMainMenu`、`WithRetryHint`、`NoItemsToProcess`。
  - 設計取捨：不把所有訊息集中成單一檔案。只提取出現 2 處以上的文字；單一流程專屬訊息與 Parser 驗證訊息留在原處，避免 `BotMessages` 變成雜物間。
  - 編輯／刪除「沒有資料」回覆現在也會附「已回到主選單。」，與其他流程一致。
- **測試與文件**：`EditFlowHandlerTests` 的選號錯誤斷言改引用 `BotMessages`；100 項測試全數通過。同步更新 `SRS.md`（BR-034、新增 BR-036）與 `design.md`（全域指令、專案結構、回覆訊息章節）。

### Phase 23 — Redis 自動探測與記憶體快取降級（2026-10-28）

**目標**：改善本地開發與非 Redis 環境體驗。原本無條件註冊 Redis 會導致未開 Redis 時一收到訊息即拋出連線例外與 500 錯誤。

- **實作改動**：
  - 在 `Program.cs` 調整分散式快取註冊邏輯：優先檢查 `RedisConnection` 連線字串設定（支援 `none` / `memory` 排除設定）。
  - 若有配置 Redis，於啟動時加入輕量 1 秒逾時連線探測（`StackExchange.Redis` `ConnectTimeout = 1000`、`AbortOnConnectFail = true`）。
  - 探測失敗或未配置時，自動降級註冊微軟內建之 `AddDistributedMemoryCache`，並印出提示訊息。
- **效益**：
  - 本地開發者毋須強制安裝或啟動 Docker Redis 即可順暢除錯與測試 LINE Bot。
  - 正式或容器環境在配置可用之 Redis 服務時，無縫保有跨實例分散式快取特性。
- **測試**：全數 100 項單元與整合測試全數通過（`Passed: 100, Failed: 0`）。

### Phase 24 — LINE Quick Reply 按鈕化（2026-10-29，分支 `feat/AddingQuickReply`）

**目標**：讓使用者靠點按鈕操作主選單、類別、確認與退出，降低打字成本（CP 值最高的 UX 改善）。

- **設計決策**：
  - 採 `message` action：點按鈕＝使用者送出同樣文字，`BotCommandParser`、狀態機、啟動流程完全不用改。
  - 只用 Reply API（免費），不用 Push Message；逾時不主動通知，使用者下次傳訊時才被動告知。
  - 項目過多（> 11）時不硬塞按鈕，退回「返回主選單／再見小幫手」，使用者仍可直接打編號。
  - 未採用 Rich Menu：全域 Rich Menu 會常駐，與「喚醒後才出現、逾時消失」需求衝突，需 per-user 綁定與排程解除，留待之後評估。
- **實作改動**：
  - `ILineClient` / `LineClient`：`ReplyMessageAsync` 新增 `quickReplies` 參數；新增 `LineQuickReply`、`LineQuickReplyItem`、`LineAction` DTO；上限 13 顆、label 20 字截斷。
  - `BotResponse` 新增 `QuickReplyOptions`；`LineWebhookController` 轉傳。
  - `BotMessages` 新增 `MainMenuQuickReplies`、`WakeUpQuickReplies`、`ReturnOrSleepQuickReplies`、`ContinueOrReturnQuickReplies`。
  - `ProcessMessageCommandHandler` 與 Save／Query／Edit／Delete FlowHandler 每個回覆點依狀態附按鈕。
- **測試**：`FakeLineClient` 新增 `SentQuickReplies`；`LineWebhookTests` 新增 `QuickReply_ShouldAttachButtonsToResponses`；106 項測試全數通過。
- **文件**：`SRS.md` 新增 FR-020／BR-044~046／AC-020-01~04；`design.md` 新增 Quick Reply 章節；新增 `usage-flow.md` 使用流程。
- **LINE 後台**：不需額外設定，確認 Webhook 開啟、自動回應訊息關閉即可。

---

## ✅ SRS 需求實作進度

| 需求 | 說明 | 狀態 |
|------|------|------|
| FR-001 | 啟動 Bot 與主選單歡迎訊息 | ✅ 已完成（主選單已支援指令提示與歡迎訊息） |
| FR-002 | 指令選擇 | ✅ 已完成（支援 Save、Query、Edit、Delete 與 Return） |
| FR-003 | 對話狀態管理 | ✅ 已完成（支援連續操作，完成後回到 MainMenu） |
| FR-004 | Return 指令 | ✅ 已完成（全域攔截「返回主選單」，任何 Flow 皆可回主選單並重新顯示喚醒提示；各回覆附下一步提示，BR-036） |
| FR-005 | 解析 URL / Name / Category | ✅ 已完成（格式 `URL #Category Name`，BR-030） |
| FR-006 | 儲存旅遊資訊 | ✅ 已完成 |
| FR-007 | 使用者自訂分類 | ✅ 已完成（直接採用使用者輸入） |
| FR-008 | 顯示可查詢分類 | ✅ 已完成（動態列出目前地區資料庫現有分類，無固定白名單） |
| FR-009 | 依分類查詢 | ✅ 已完成 |
| FR-010 | 缺 URL 或格式無效的處理 | ✅ 已完成（檢查缺少與 http/https 格式驗證） |
| FR-011 | 缺 Category 的處理 | ✅ 已完成 |
| FR-012 | 輸入格式錯誤的處理 | ✅ 已完成（提供錯誤原因並附上正確格式範例） |
| FR-013 | 缺 Name 的處理 | ✅ 已完成 |
| FR-014 | 編輯 | ✅ 已完成（支援 URL、類別、名稱任一單獨修改或多項組合修改，包含重複名稱確認） |
| FR-015 | 刪除 | ✅ 已完成（包含確認流程） |
| FR-016 | 名稱重複確認 | ✅ 已完成（繼續 / 返回主選單 確認流程） |
| FR-017 | 切換地區 | ✅ 已完成（`切換地區` / `切換目的地`，資料依地區隔離） |
| FR-018 | 流程逾時 | ✅ 已完成（非 Idle 超過 10 分鐘自動重置，Redis 1 天過期） |
| FR-019 / — | LINE Webhook 串接 | ✅ 已完成（`LineWebhookController` 支援 HMAC-SHA256 簽章驗證、MediatR 轉發與 LINE 回覆 API） |
| FR-020 | LINE Quick Reply 按鈕 | ✅ 已完成（依狀態附按鈕，上限 13 顆；項目 > 11 時退回文字輸入） |

---

## 🧹 已知問題與技術債


---

## 🔜 下一步規劃

1. **本機外網測試與 LINE 帳號連通**：將已完成的 Webhook API 透過 **ngrok** 暴露至外網，並於 LINE Developers Console 設定 Webhook URL，實際以 LINE 軟體測試所有互動流程。(目前未設置ngrok, 已將部屬遷移至 [NorthFlank](https://northflank.com/)，有提供免費的Webtwork Tunnel)
2. **優化使用者體驗 (UX)**：考慮利用 LINE Flex Message 美化查詢結果，或加入「系統支援的分類」白名單（解決 FR-008 延伸問題），降低使用者輸入錯誤的機率。
3. **完善例外處理**：加入 URL 格式驗證功能（改善 FR-010）與完整的錯誤訊息範例提示（改善 FR-012）。
4. **自動化 CI/CD 部署**：建立 GitHub Actions CI（包含自動化單元測試與建置），並整合部署至 **NorthFlank** 等雲端平台。

---

## 📝 紀錄方式

之後每完成一個功能或做出重要決策，請在本文件中：

1. 在「時間軸總覽」新增一列（日期、Commit、階段、摘要）。
2. 在「各階段詳細紀錄」新增一個 Phase 段落，寫下做了什麼、為什麼這樣做。
3. 更新「SRS 需求實作進度」與「已知問題與技術債」。
