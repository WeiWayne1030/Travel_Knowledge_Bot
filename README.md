# 🌺 Okinawa Travel Knowledge Bot (沖繩旅遊知識庫 LINE Bot)

這是一個專為沖繩旅遊知識庫打造的後端 API / LINE Bot 服務，基於 **.NET 9 Web API** 與 **SQLite** 資料庫開發。

---

## 🛠️ 環境準備 (Prerequisites)

在開始安裝與執行前，請確保您的開發環境已安裝以下工具：

1. **[.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)** (建議版本 9.0.100 或以上)
2. **Git**
3. **SQLite** (專案內建使用 `okinawa.db`，可搭配 [SQLite Browser](https://sqlitebrowser.org/) 或 VS Code 擴充套件進行資料檢視)
4. *(選用)* **EF Core CLI 工具** (若需要執行資料庫 Migration)：
   ```bash
   dotnet tool install --global dotnet-ef
   ```

---

## 📥 專案 Clone 與安裝流程 (Setup Instructions)

### 1. Clone 儲存庫 (Clone Repository)

開啟終端機 (Terminal / Command Prompt / PowerShell)，執行以下指令將專案複製到本地：

```bash
git clone https://github.com/WeiWayne1030/Travel_Knowledge_Bot.git
cd Travel_Knowledge_Bot
```

---

### 2. 還原套件 (Restore Packages)

在專案根目錄下執行以下指令，還原所有 NuGet 套件與依賴項：

```bash
dotnet restore
```

---

### 3. 設定資料庫與設定檔 (Configuration & Database Setup)

#### 設定檔說明
應用程式的設定檔位於 `src/OkinawaBot/appsettings.json`。預設已設定連線字串至同目錄下的 SQLite 資料庫 `okinawa.db`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=okinawa.db"
  }
}
```

#### 資料庫移轉 (Entity Framework Core Migrations)
若需要更新或初始化資料庫結構，請切換至 `src/OkinawaBot` 目錄並執行：

```bash
cd src/OkinawaBot
dotnet ef database update
```

*(注意：專案中已包含預設的 `okinawa.db` 檔案，若直接啟動通常無須額外手動更新移轉。)*

---

## 🚀 如何運行與測試 (Running the Application)

### 1. 啟動專案 (Run Application)

從根目錄執行以下指令啟動 Web API 服務：

```bash
dotnet run --project src/OkinawaBot/OkinawaBot.csproj
```

或者進入專案目錄後直接運行：

```bash
cd src/OkinawaBot
dotnet run
```

啟動成功後，終端機將顯示服務監聽的 URL (例如 `http://localhost:5000` 或 `https://localhost:5001`)。

---

### 2. API 測試與呼叫 (Testing & Operation)

您可以透過以下方式驗證 API 服務：

- **使用 `.http` 檔案測試 (VS Code / Visual Studio)**：
  可以直接開啟 [src/OkinawaBot/OkinawaBot.http](src/OkinawaBot/OkinawaBot.http) 檔案點擊 `Send Request` 進行 API 測試。
- **使用 Postman / cURL**：
  對 API Endpoint 發送 HTTP 請求測試功能。

---

## 📂 專案結構 (Project Structure)

```text
Okinawa_Travel_Knowledge_Bot/
├── docs/                 # 專案文件 (如 SRS 規格書)
├── src/
│   └── OkinawaBot/       # 主程式專案
│       ├── Application/  # 應用程式邏輯 (Services, Handlers)
│       ├── Controllers/  # API 控制器
│       ├── Domain/       # 領域模型與介面
│       ├── Infrastructure/# 資料庫與外部服務實作 (Data, LINE API)
│       ├── Migrations/   # EF Core 資料庫移轉檔
│       ├── Models/       # 資料模型
│       ├── Program.cs    # 程式進入點與 DI 設定
│       └── okinawa.db    # SQLite 資料庫檔案
├── global.json           # .NET SDK 版本控制
└── OkinawaBot.sln        # 解決方案檔
```
