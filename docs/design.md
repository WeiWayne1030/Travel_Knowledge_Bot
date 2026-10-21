# Okinawa Travel Knowledge Bot — 系統設計文件

> 本文件以**目前程式碼**為準。SRS 撰寫原則：
> FR = 系統要做什麼　BR = 系統有哪些規則　AC = 怎樣才算完成

## 1. 對話狀態機

`ConversationState` 共 11 種狀態（`Application/States/ConversationState.cs`）。

```mermaid
stateDiagram-v2
    [*] --> Idle
    Idle --> MainMenu: 旅遊小幫手
    Idle --> Idle: 其他訊息（不回覆）

    MainMenu --> SaveFlow: 儲存
    MainMenu --> QueryFlow: 查詢
    MainMenu --> EditItemSelection: 編輯
    MainMenu --> DeleteFlow: 刪除
    MainMenu --> SwitchDestination: 切換地區 / 切換目的地

    SwitchDestination --> MainMenu: 選號碼或輸入新地區

    SaveFlow --> SaveDuplicateConfirmation: 同地區名稱重複
    SaveFlow --> MainMenu: 儲存成功
    SaveDuplicateConfirmation --> MainMenu: 繼續（儲存）

    QueryFlow --> MainMenu: 查到結果
    QueryFlow --> QueryFlow: 查無資料，重新輸入

    EditItemSelection --> EditDataInput: 輸入項目編號
    EditDataInput --> EditDuplicateConfirmation: 新名稱重複
    EditDataInput --> MainMenu: 修改成功
    EditDuplicateConfirmation --> MainMenu: 繼續（儲存）

    DeleteFlow --> DeleteConfirmation: 輸入項目編號
    DeleteConfirmation --> MainMenu: 繼續（刪除）
```

### 全域指令（在非 Idle 狀態皆優先攔截）

| 指令 | 行為 |
|------|------|
| `返回主選單` | `ResetToMainMenu`（清除 CurrentItemId / PendingSave / PendingEdit，保留目的地）→ `MainMenu`，並重新顯示喚醒提示 |
| `再見小幫手` | `Reset` → `Idle`，回覆「已退出小幫手。」 |

### 逾時與連續操作

- 流程完成後（儲存／編輯／刪除成功、查詢出結果）狀態回到 `MainMenu`（連續操作模式），回覆訊息附帶主選單提示，使用者不需重新輸入「旅遊小幫手」即可繼續操作。
- 非 `Idle` 狀態超過 **10 分鐘**（`FlowTimeout`）未活動，下一則訊息先重置流程：
  - 訊息為 `旅遊小幫手`：直接回主選單（保留目的地）。
  - 其他訊息：回覆「先前的操作已逾時並已重置，請輸入「旅遊小幫手」開始。」
- Redis 快取為 **1 天**滑動過期（目的地偏好長效保留）。

## 2. 系統架構

```mermaid
flowchart TD
    U[LINE User] --> P[LINE Platform]
    P -->|Webhook POST /api/LineWebhook| C[LineWebhookController<br/>HMAC-SHA256 簽章驗證]
    C -->|ProcessMessageCommand| M[MediatR]
    M --> H[ProcessMessageCommandHandler<br/>喚醒/休眠/逾時/全域指令/主選單]
    H <--> S[(Redis<br/>ConversationStateManager)]
    H -->|Save/Query/Edit/DeleteFlowCommand| F[Flow Handlers]
    F --> V[Services<br/>Save / Query / Edit / Delete]
    V --> R[TravelItemRepository]
    R --> D[(SQLite<br/>okinawa.db)]
    C -->|ReplyMessageAsync| L[LineClient]
    L --> P
```

## 3. 專案結構

```
OkinawaBot/
├── src/OkinawaBot/
│   ├── Controllers/
│   │   └── LineWebhookController.cs
│   ├── Application/
│   │   ├── Commands/        BotCommand, BotCommandParser
│   │   ├── Requests/        ProcessMessageCommand, Save/Query/Edit/DeleteFlowCommand
│   │   ├── Handlers/        ProcessMessageCommandHandler
│   │   ├── Flows/           Save/Query/Edit/DeleteFlowHandler
│   │   ├── Input/           SaveInputParser, EditInputParser
│   │   ├── Services/        Save/Query/Edit/DeleteService
│   │   ├── Models/          BotResponse, SaveResult, EditResult, 各種 Request/ParseResult DTO
│   │   └── States/          ConversationState, ConversationContext, ConversationStateManager
│   ├── Domain/
│   │   ├── Entities/        TravelItem
│   │   └── Interfaces/      ITravelItemRepository
│   ├── Infrastructure/
│   │   ├── Data/            AppDbContext
│   │   ├── Repositories/    TravelItemRepository
│   │   └── Line/            ILineClient, LineClient, LineSignatureValidator,
│   │                        LineBotOptions, LineWebhookRequest
│   ├── Migrations/
│   └── Program.cs
├── tests/OkinawaBot.Tests/
├── docs/                    SRS.md, design.md, DEVLOG.md
├── .github/workflows/       dotnet.yml (GitHub Actions CI)
├── docker-compose.yml       okinawabot + redis
├── Dockerfile
├── OkinawaBot.sln
└── README.md
```

## 4. 指令與輸入格式

| 狀態 | 輸入 |
|------|------|
| Idle | `旅遊小幫手` 喚醒 |
| MainMenu | `儲存` / `查詢` / `編輯` / `刪除` / `切換地區`（或 `切換目的地`） |
| SaveFlow | `URL #類別 名稱`（單行或多行；`#類別` 前為 URL、後為名稱；URL 必須以 http:// 或 https:// 開頭） |
| QueryFlow | 類別名稱（需與資料庫現有類別完全相符，限定目前地區） |
| EditItemSelection / DeleteFlow | 項目編號（進入流程時會動態列出目前地區的所有項目與 1-based 編號供選擇） |
| EditDataInput | 單項或多項：`URL`、`#類別`、`名稱`，或 `網址:` / `類別:` / `名稱:` 前綴（URL 必須以 http:// 或 https:// 開頭） |
| 各確認狀態 | `繼續` 或 `返回主選單` |

URL 驗證：Save 與 Edit 均已實作 URL 合法 Scheme (http/https) 格式驗證。

## 5. SQLite 資料表

```
┌────────────────────────────────────────┐
│              travel_items              │
├────────────────────────────────────────┤
│ Id           INTEGER PK                │
│ Destination  TEXT NOT NULL  (預設 沖繩) │
│ Name         TEXT NOT NULL             │
│ Url          TEXT NOT NULL             │
│ Category     TEXT NOT NULL             │
│ CreatedAt    DATETIME                  │
│ UpdatedAt    DATETIME                  │
├────────────────────────────────────────┤
│ INDEX (Destination, Category)          │
│ INDEX (Destination, Name)              │
└────────────────────────────────────────┘
```

- 資料不綁定 LINE 使用者（所有使用者共用同一份資料）。
- 啟動時自動執行 `Database.Migrate()`。

## 6. 對話狀態（Redis）

`ConversationContext`（JSON 序列化，Key 為 LINE `userId`，InstanceName 前綴 `OkinawaBot_`）：

| 欄位 | 說明 |
|------|------|
| `UserId` | LINE userId |
| `State` | 目前 `ConversationState` |
| `CurrentDestination` | 目前地區，預設 `沖繩` |
| `CurrentItemId` | 編輯／刪除中的項目 Id |
| `PendingSave` | 等待重複名稱確認的儲存請求 |
| `PendingEdit` | 等待重複名稱確認的編輯請求 |
| `LastActiveAt` | 最後活動時間（逾時判斷用） |