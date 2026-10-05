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

---

## ✅ SRS 需求實作進度

| 需求 | 說明 | 狀態 |
|------|------|------|
| FR-001 | 啟動 Bot 與主選單歡迎訊息 | ✅ 已完成（主選單已支援指令提示與歡迎訊息） |
| FR-002 | 指令選擇 | 🟡 已能切換狀態，但只有 Save Flow 有實作 |
| FR-003 | 對話狀態管理 | ✅ 已完成（已支援進入 Save Flow 的格式提示 AC-003-01） |
| FR-004 | Return 指令 | 🟡 Save Flow 已支援，其他 Flow 還沒有 |
| FR-005 | 解析 URL / Name / Category | ✅ 已完成（格式 `URL #Category Name`，BR-030） |
| FR-006 | 儲存旅遊資訊 | ✅ 已完成 |
| FR-007 | 使用者自訂分類 | ✅ 已完成（直接採用使用者輸入） |
| FR-008 | 顯示可查詢分類 | ⬜ 未實作 |
| FR-009 | 依分類查詢 | ⬜ 未實作（Repository 已有 `FindByCategoryAsync`） |
| FR-010 | 缺 URL 的處理 | 🟡 有檢查缺少，但不驗證 URL 格式 |
| FR-011 | 缺 Category 的處理 | ✅ 已完成 |
| FR-012 | 輸入格式錯誤的處理 | 🟡 有錯誤訊息，但沒有附上正確格式範例 |
| FR-013 | 缺 Name 的處理 | ✅ 已完成 |
| FR-014 | 編輯 | ⬜ 未實作 |
| FR-015 | 刪除 | ⬜ 未實作 |
| FR-016 | 名稱重複確認 | ✅ 已完成（Continue / Return 確認流程） |
| — | LINE Webhook 串接 | ⬜ 未實作（`Controllers/`、`Models/`、`Infrastructure/Line/` 目前是空的） |

---

## 🧹 已知問題與技術債

- 還沒有「系統支援的分類」白名單（FR-008 / BR-013 需要）。
- `.github/workflows/` 目錄是空的，還沒有 CI。

---

## 🔜 下一步規劃

1. 實作 Query Flow（FR-008、FR-009）。
2. 實作 Edit Flow（FR-014）和 Delete Flow（FR-015）。
3. 串接 LINE Messaging API：`LineWebhookController`、簽章驗證、`LineClient` 回覆訊息。
4. 建立 GitHub Actions CI（`dotnet build` + `dotnet test`）。

---

## 📝 紀錄方式

之後每完成一個功能或做出重要決策，請在本文件中：

1. 在「時間軸總覽」新增一列（日期、Commit、階段、摘要）。
2. 在「各階段詳細紀錄」新增一個 Phase 段落，寫下做了什麼、為什麼這樣做。
3. 更新「SRS 需求實作進度」與「已知問題與技術債」。
