# SRS撰寫原則:
FR = 系統要做什麼
BR = 系統有哪些規則
AC = 怎樣才算完成

                         │  MAIN_MENU   │
                         └──────┬───────┘
                                │
          ┌────────────┬────────┼────────────┐
          │            │        │            │
        Save         Query     Edit        Delete
          │            │        │            │
          ▼            ▼        ▼            ▼
     SAVE_FLOW    QUERY_FLOW EDIT_FLOW DELETE_FLOW
          │            │        │            │
          │            │        │            │
          ▼            ▼        ▼            ▼
      Parse Input   Category   Item         Item
          │         Selection  Selection    Selection
          │            │        │            │
          ▼            ▼        ▼            ▼
      Validate       Query DB  Edit Field  Confirm
          │            │        │            │
          ▼            ▼        ▼            ▼
      Duplicate      Results   Validate    Delete
        Check                     │
          │                       ▼
          ▼                  Duplicate Check
       Save DB                    │
          │                       ▼
          │                    Update DB
          │                       │
          └────────────┬──────────┴────────────┐
                       │                       │
                       │      Return           │
                       └───────────┬───────────┘
                                   ▼
                            ┌──────────────┐
                            │  MAIN_MENU   │
                            └──────────────┘


# System Architecture

┌─────────────────────┐
│       LINE User     │
└──────────┬──────────┘
           │ Message
           ▼
┌─────────────────────┐
│     LINE Platform   │
└──────────┬──────────┘
           │ Webhook
           ▼
┌─────────────────────┐
│   Webhook Controller│
└──────────┬──────────┘
           ▼
┌─────────────────────┐
│   Message Handler   │
└──────────┬──────────┘
           ▼
┌─────────────────────┐
│ Command / State     │
│ Manager             │
└──────────┬──────────┘
           ▼
┌─────────────────────┐
│      Service        │
│                     │
│ Save / Query /      │
│ Edit / Delete       │
└──────────┬──────────┘
           ▼
┌─────────────────────┐
│     Repository      │
└──────────┬──────────┘
           ▼
┌─────────────────────┐
│       SQLite        │
└─────────────────────┘

# Repository structure
OkinawaBot/
│
├── src/
│   └── OkinawaBot/
│       │
│       ├── Controllers/
│       │   └── LineWebhookController.cs
│       │
│       ├── Application/
│       │   │
│       │   ├── Handlers/
│       │   │   └── MessageHandler.cs
│       │   │
│       │   ├── Services/
│       │   │   ├── SaveService.cs
│       │   │   ├── QueryService.cs
│       │   │   ├── EditService.cs
│       │   │   └── DeleteService.cs
│       │   │
│       │   └── State/
│       │       ├── ConversationState.cs
│       │       └── ConversationStateManager.cs
│       │
│       ├── Domain/
│       │   ├── Entities/
│       │   │   └── TravelItem.cs
│       │   │
│       │   ├── Interfaces/
│       │   │   └── ITravelItemRepository.cs
│       │   │
│       │   └── Rules/
│       │
│       ├── Infrastructure/
│       │   │
│       │   ├── Data/
│       │   │   └── AppDbContext.cs
│       │   │
│       │   ├── Repositories/
│       │   │   └── TravelItemRepository.cs
│       │   │
│       │   └── Line/
│       │       └── LineClient.cs
│       │
│       ├── Models/
│       │   └── LineWebhookRequest.cs
│       │
│       └── Program.cs
│
├── tests/
│   └── OkinawaBot.Tests/
│
├── docs/
│   ├── SRS.md
│   ├── architecture.md
│   ├── state-diagram.md
│   └── database.md
│
├── OkinawaBot.sln
└── README.md

# SQLite 規劃
┌──────────────────────────────┐
│        travel_items          │
├──────────────────────────────┤
│ id            INTEGER PK     │
│ name          TEXT NOT NULL  │
│ url           TEXT NOT NULL  │
│ category      TEXT NOT NULL  │
│ created_at    DATETIME       │
│ updated_at    DATETIME       │
└──────────────────────────────┘