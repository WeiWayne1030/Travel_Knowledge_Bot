# 📓 開發歷程紀錄 (Development Log)

本文件記錄 **Okinawa Travel Knowledge Bot** 從需求分析到實作的開發歷程，方便日後回顧設計決策、追蹤進度與規劃下一步。

> 最後更新：2026-10-05
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
| 9 | 2026-10-06 | 待定 | 補齊所有流程與環境修復 | 完成 Query、Edit、Delete Flow，修復 MessageHandler 測試，並修正 Swagger 在 .NET 9 下的衝突與啟動設定 |
| 10 | 2026-10-06 | 待定 | 補齊所有流程的整合測試 | 在 `LineWebhookTests.cs` 中補上 Query、Edit、Delete Flow 的 Webhook 整合測試，處理測試間 DB 資料互相污染問題。目前 64 測試全數通過 |
| 11 | 2026-10-06 | 待定 | 引入 MediatR 架構重構 | 為了解耦 Controller 與 Handler，引入 MediatR 套件，將所有 FlowHandler 重構為 `IRequestHandler`，並將主邏輯移至 `ProcessMessageCommandHandler`，全面修復單元測試。 |

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

### Phase 8 — 技術債與命名清理

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

### Phase 9 — 補齊 Query、Edit、Delete 流程與修復 Swagger

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

### Phase 10 — 補齊所有流程的整合測試

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

### Phase 11 — 引入 MediatR 架構重構

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

---

## ✅ SRS 需求實作進度

| 需求 | 說明 | 狀態 |
|------|------|------|
| FR-001 | 啟動 Bot 與主選單歡迎訊息 | ✅ 已完成（主選單已支援指令提示與歡迎訊息） |
| FR-002 | 指令選擇 | ✅ 已完成（支援 Save、Query、Edit、Delete 與 Return） |
| FR-003 | 對話狀態管理 | ✅ 已完成（已支援進入各 Flow 的格式提示 AC-003-01） |
| FR-004 | Return 指令 | ✅ 已完成（所有 Flow 皆支援返回） |
| FR-005 | 解析 URL / Name / Category | ✅ 已完成（格式 `URL #Category Name`，BR-030） |
| FR-006 | 儲存旅遊資訊 | ✅ 已完成 |
| FR-007 | 使用者自訂分類 | ✅ 已完成（直接採用使用者輸入） |
| FR-008 | 顯示可查詢分類 | ✅ 已完成 |
| FR-009 | 依分類查詢 | ✅ 已完成 |
| FR-010 | 缺 URL 的處理 | 🟡 有檢查缺少，但不驗證 URL 格式 |
| FR-011 | 缺 Category 的處理 | ✅ 已完成 |
| FR-012 | 輸入格式錯誤的處理 | 🟡 有錯誤訊息，但沒有附上正確格式範例 |
| FR-013 | 缺 Name 的處理 | ✅ 已完成 |
| FR-014 | 編輯 | ✅ 已完成（包含確認流程） |
| FR-015 | 刪除 | ✅ 已完成（包含確認流程） |
| FR-016 | 名稱重複確認 | ✅ 已完成（Continue / Return 確認流程） |
| — | LINE Webhook 串接 | ⬜ 未實作（`Controllers/`、`Models/`、`Infrastructure/Line/` 目前是空的） |

---

## 🧹 已知問題與技術債

- 還沒有「系統支援的分類」白名單（FR-008 / BR-013 需要）。
- `.github/workflows/` 目錄是空的，還沒有 CI。

---

## 🔜 下一步規劃

1. 串接 LINE Messaging API：實作 `LineWebhookController`、簽章驗證、以及使用 `LineClient` 進行訊息回覆。
2. 加入 URL 格式驗證功能（改善 FR-010）與完整的錯誤訊息範例（改善 FR-012）。
3. 建立 GitHub Actions CI（包含 `dotnet build` + `dotnet test`）。
4. 考慮加入「系統支援的分類」白名單或輔助（解決 FR-008 延伸問題）。

---

## 📝 紀錄方式

之後每完成一個功能或做出重要決策，請在本文件中：

1. 在「時間軸總覽」新增一列（日期、Commit、階段、摘要）。
2. 在「各階段詳細紀錄」新增一個 Phase 段落，寫下做了什麼、為什麼這樣做。
3. 更新「SRS 需求實作進度」與「已知問題與技術債」。
