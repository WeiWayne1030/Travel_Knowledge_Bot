# Okinawa Travel Knowledge Bot
## Software Requirements Specification

## 1. Introduction
Okinawa Travel Knowledge Bot is a personal travel information management system integrated with LINE.

The system allows users to save travel-related URLs through LINE with a user-specified category, and to query, edit and delete previously saved travel information. Information is organized by destination (default: 沖繩) and category.

## 2. Problem Statement
The user currently stores Okinawa travel information by sending URLs to a LINE group.

However, the information is not categorized or structured.
As the number of saved URLs increases, it becomes difficult to:

1. Identify what each URL represents.
2. Find previously saved information.
3. Group information by category.
4. Search information by location or topic.
5. Reuse the collected information when planning the itinerary.

## 3. Goals
The system aims to:

1. Allow users to save travel URLs through LINE.
2. Organize saved travel information by user-defined categories.
3. Store the URL, name, category and destination of each travel item.
4. Allow users to query saved information by category within the current destination.
5. Allow users to edit and delete saved information, and to switch between destinations.
6. Reduce the effort required to manually organize travel information.

## 4. Scope
### 4.1 In Scope

The MVP includes:

- Receiving messages from LINE.
- Detecting URLs in messages.
- Parsing the URL, name and category from the user's input.
- Switching between destinations (e.g. 沖繩, 東京, 首爾).
- Allowing users to specify a category manually.
- Storing travel information.
- Querying saved travel information by category.
- Editing and deleting saved travel information.
- Conversation timeout handling and persistent conversation state (Redis).

### 4.2 Out of Scope

The MVP does not include:

- AI-based classification or automatic metadata extraction from URLs.
- Keyword search across categories.
- Automatic itinerary generation.
- Hotel or restaurant reservations.
- Navigation.
- Real-time travel recommendations.
- Multi-user collaboration and LINE group chat support. Conversation state is keyed by the LINE `userId`; saved travel items are not linked to a user, so all users of the same deployment share the same data.
- Mobile applications.
- A dedicated web frontend.
- Advanced recommendation algorithms.
## 5. Users
### 5.1 Personal User

The system is initially designed for a single personal user who collects and organizes travel information through LINE one-on-one chat.

## 6. Functional Requirements
FR-001 — Bot Activation and Main Menu

The system SHALL provide a bot activation mechanism. While the bot is inactive (Idle), the system SHALL ignore all messages (no reply) except `旅遊小幫手`. When the user activates the bot by typing `旅遊小幫手`, the system SHALL display a welcome message that includes the current destination and the available commands. The user can type `再見小幫手` in any state to deactivate the bot.

Available commands:
- 儲存 (Save)
- 查詢 (Query)
- 編輯 (Edit)
- 刪除 (Delete)
- 切換地區 / 切換目的地 (Switch Destination)

Acceptance Criteria

<!-- 使用者打字時激活ai -->
AC-001-01

Given the bot is inactive
When the user activates the bot
Then the system SHALL display a welcome message.

<!-- 提供增刪改查的提示 -->
AC-001-02

Given the bot has been activated
When the welcome message is displayed
Then the system SHALL provide the available commands: Save, Query, Edit, Delete and Switch Destination, and SHALL show the current destination.

FR-002 — Command Selection

The system SHALL allow the user to select an operation from the Main Menu by entering one of the supported commands: 儲存, 查詢, 編輯, 刪除, or 切換地區 (alias 切換目的地). Any other input in the Main Menu SHALL cause the system to display the list of supported commands.

The user SHALL be able to select one of these functions.

Business Rules

<!-- 第一版先只支援增刪改查-->
BR-001

The system SHALL support the 儲存, 查詢, 編輯, 刪除 and 切換地區 functions in the initial version.

BR-002

The system SHALL execute the flow corresponding to the function selected by the user.
Acceptance Criteria

AC-002-01

Given the user has activated the bot
When the user selects 儲存
Then the system SHALL enter the Save flow.

AC-002-02

Given the user has activated the bot
When the user selects 查詢
Then the system SHALL enter the Query flow.

AC-002-03

Given the user has activated the bot
When the user selects 編輯
Then the system SHALL enter the Edit flow.

AC-002-04

Given the user has activated the bot
When the user selects 刪除
Then the system SHALL enter the Delete flow.

AC-002-05

Given the user has activated the bot
When the user selects 切換地區
Then the system SHALL enter the Switch Destination flow (see FR-017).

FR-003 — Conversation State Management

The system SHALL maintain the user's current conversation state and process messages according to the active state.

Save input format:

URL #Category Name

Example (single line):

https://example.com/okinawa #Attraction Danny Food Store

Example (multiple lines):

https://example.com/okinawa
#Attraction
Danny Food Store

Business Rules

BR-028

A Save request SHALL contain a valid URL, name and a category.

BR-029

The category SHALL be specified using the #Category format.

BR-030

The text before #Category SHALL be treated as the URL, and the text after #Category SHALL be treated as the name.
Acceptance Criteria

AC-003-01

Given the user selects Save
When the Save flow starts
Then the system SHALL display the expected input format.

AC-003-02

Given the user is in the Save flow
When the user provides a URL, #Category and name in the format URL #Category Name
Then the system SHALL proceed to input validation.

FR-004 — Return Command

The system SHALL allow the user to enter `返回主選單` from any interactive flow and return to the Main Menu.

Business Rules

BR-003

The `Return` command SHALL be available in all interactive states except the Main Menu.

BR-004

When the system receives `Return`, it SHALL discard the current operation state (current item, pending save and pending edit), keep the current destination, return the user to the Main Menu and re-display the welcome message.

BR-034

The system SHALL also accept `再見小幫手` in any non-Idle state; it SHALL reset the conversation to Idle and reply `已退出小幫手。`.

BR-035

When a Save, Edit or Delete flow completes (success or failure), or a Query returns results, the conversation SHALL return to Idle. The user needs to type `旅遊小幫手` again to continue.

Acceptance Criteria

AC-004-01

Given the user is in the Save flow
When the user enters `返回主選單`
Then the system SHALL return the user to the Main Menu.

AC-004-02

Given the user is in the Query flow
When the user enters `返回主選單`
Then the system SHALL return the user to the Main Menu.

AC-004-03

Given the user is in the Edit flow
When the user enters `返回主選單`
Then the system SHALL return the user to the Main Menu.

AC-004-04

Given the user is in the Delete flow
When the user enters `返回主選單`
Then the system SHALL return the user to the Main Menu.

FR-005 — Parse URL, name and Category

The system SHALL identify and extract the URL, name and category from the user's input.

Business Rules

BR-005

The system SHALL extract the URL and name from the user's message.

BR-006

The system SHALL extract the category specified after #.

BR-007

The system SHALL treat the category explicitly provided by the user as user-defined input.
Acceptance Criteria

AC-005-01

Given the user provides a valid URL, name and category
When the system processes the message
Then the system SHALL extract the URL, name and category successfully.

AC-005-02

Given the user provides multiple pieces of text together with a valid URL, name and category
When the system processes the message
Then the system SHALL extract the URL, name and category if all can be identified.

FR-006 — Store Travel Information

The system SHALL store the user's URL, name and category in the database after successfully parsing the input.

Business Rules

BR-008

The system SHALL only store travel information after successful input validation.

BR-009

Each saved item SHALL contain a URL, name and category.
Acceptance Criteria

AC-006-01

Given the URL, name and category are valid
When the system completes parsing
Then the system SHALL store the URL, name and category in the database.

AC-006-02

Given the URL, name or category is invalid or missing
When the system processes the input
Then the system SHALL NOT store the item.

AC-006-03

Given the item has been successfully stored
When the save operation is completed
Then the system SHALL notify the user that the item was successfully saved and SHALL store the item under the current destination.

FR-007 — User-Defined Category

The system SHALL use the category explicitly provided by the user as the final category.

The system SHALL NOT use AI or URL domain-based rules to determine or override the user's specified category.

Business Rules

BR-010

The user's explicitly specified category SHALL always be the final category.

BR-011

The system SHALL NOT override the user's category based on the URL domain.

BR-012

The system SHALL NOT use AI to classify the category in the initial version.
Acceptance Criteria

AC-007-01

Given the URL belongs to a hotel-related website
When the user specifies #Attraction
Then the system SHALL store the category as Attraction.

AC-007-02

Given the URL belongs to a restaurant-related website
When the user specifies #Shopping
Then the system SHALL store the category as Shopping.

FR-008 — Display Query Categories

When the user selects the Query function (查詢), the system SHALL dynamically fetch and display the available unique categories from the database.

Example categories may include:

Hotel
Restaurant
Attraction
Shopping
Transportation
Rental Car
Flight
Business Rules

BR-013

The displayed categories SHALL be the distinct categories already stored in the database for the current destination. There is no fixed category whitelist.

BR-014

The Query flow SHALL match the entered category text against the stored categories of the current destination. A category that has no stored items yields the no-match message.
Acceptance Criteria

AC-008-01

Given the user selects Query
When the Query flow starts
Then the system SHALL display the available categories.

AC-008-02

Given the category list is displayed
When the user selects a supported category
Then the system SHALL proceed with the category query.

FR-009 — Query Travel Information by Category

The system SHALL retrieve and return travel information based on the category selected by the user.

The returned information SHALL include the current destination, the selected category (in the result header), and the stored name and URL of each item, numbered from 1.

Business Rules

BR-015

The system SHALL only return items matching the selected category.

BR-016

The system SHALL return the name and URL associated with each matching item.

BR-017

If no items exist for the selected category in the current destination, the system SHALL notify the user that no matching items were found, SHALL ask the user to re-enter a category, and SHALL remain in the Query flow.
Acceptance Criteria

AC-009-01

Given the database contains items with the Attraction category
When the user selects Attraction
Then the system SHALL return the stored Attraction items.

AC-009-02

Given the database contains no items with the selected category
When the user performs the query
Then the system SHALL notify the user that no matching items were found.

AC-009-03

Given multiple items match the selected category
When the user performs the query
Then the system SHALL return all matching items in a single message and end the Query flow (the conversation returns to Idle).

FR-010 — Missing or Invalid URL Handling

If the user submits an input without a valid URL or with an invalid URL format, the system SHALL display an error message and reject the input. The URL MUST use http:// or https://.

Example:

URL #Category Name

Business Rules

BR-018

A Save request without a valid URL SHALL NOT be stored.
Acceptance Criteria

AC-010-01

Given the user is in the Save flow
When the user submits only #Attraction
Then the system SHALL display a message indicating that the URL is missing.

AC-010-02

Given the URL is missing
When the system displays the error message
Then the system SHALL tell the user to place the URL before #Category.
(Current limitation: the full `URL #Category Name` example is shown when entering the Save flow, not in this error message.)

FR-011 — Missing Category Handling

If the user submits a valid URL without a category, the system SHALL display an error message and request the user to provide a category.

Business Rules

BR-019

A Save request without a category SHALL NOT be stored.
Acceptance Criteria

AC-011-01

Given the user is in the Save flow
When the user submits a valid URL without a category
Then the system SHALL display a message indicating that the category is missing.

AC-011-02

Given the category is missing
When the system displays the error message
Then the system SHALL request the user to provide a category using the #Category format.

FR-012 — Invalid Input Format Handling

If the user's input does not match the expected input format, the system SHALL display an error message and provide an example of a valid input format.

Business Rules

BR-020

The system SHALL reject input that cannot be parsed into the required Save input format.

BR-021

Invalid input SHALL NOT be stored in the database.
Acceptance Criteria

AC-012-01

Given the user is in the Save flow
When the user submits text that contains neither a valid URL nor a category
Then the system SHALL display an invalid input message.

AC-012-02

Given the input format is invalid
When the system displays the error message
Then the system SHALL provide a hint on how to fix the input and attach an example of a valid input format.

FR-013 — Missing Name Handling

If the user submits a valid URL and category without a name, the system SHALL display an error message and request the user to provide a name.

Business Rules

BR-022

A Save request without a name SHALL NOT be stored.

Acceptance Criteria

AC-013-01

Given the user is in the Save flow
When the user submits a valid URL and category without a name
Then the system SHALL display a message indicating that the name is missing.

AC-013-02

Given the name is missing
When the system displays the error message
Then the system SHALL request the user to provide a name.

FR-014 — Edit Travel Information

The system SHALL allow the user to select a saved item by its number (within the current destination) and update its URL, name or category. The user MAY change any single field or any combination of fields; omitted fields keep their original values. Optional field prefixes are supported: `名稱:`, `類別:` / `分類:`, `網址:` / `URL:`.

Business Rules

BR-023

The system SHALL identify the saved item selected by the user by its 1-based number in the list of all saved items of the current destination. Note: the list itself is not displayed when entering the Edit flow.

BR-024

The system SHALL validate updated values before storing the changes: a provided URL MUST use http:// or https://, a category MUST NOT be empty (`#` alone is rejected), and at least one field MUST be provided.

BR-036

If a new name different from the original already exists in the current destination, the system SHALL ask the user to enter `繼續` to save anyway or `返回主選單` to cancel.

Acceptance Criteria

AC-014-01

Given the user selects a saved item to edit
When the user provides valid updated values for one or more fields
Then the system SHALL update the selected item and confirm the change.

AC-014-02

Given the user provides invalid or incomplete updated values
When the system validates the edit
Then the system SHALL display an error and SHALL NOT update the item.

FR-015 — Delete Travel Information

The system SHALL allow the user to select and delete a saved item.

Business Rules

BR-025

The system SHALL select the item by its 1-based number in the list of all saved items of the current destination (the list is not displayed in the flow), and SHALL request confirmation by asking the user to enter `繼續` before permanently deleting it. Any other input re-displays the prompt; `返回主選單` cancels.

BR-026

The system SHALL delete only the item selected by the user.

Acceptance Criteria

AC-015-01

Given the user selects a saved item to delete
When the user confirms the deletion
Then the system SHALL delete the selected item and notify the user.

AC-015-02

Given the user is asked to confirm deletion
When the user cancels
Then the system SHALL keep the item (the user returns to the Main Menu via 返回主選單).

FR-016 — Duplicate Name Confirmation

When the user submits a Save request with a name that matches an existing saved item in the same destination, the system SHALL warn the user that the name is already in use and ask the user to confirm whether to continue saving. The same name MAY exist in different destinations without a warning.

The user SHALL respond with one of the following commands:
- 繼續 (Continue)
- 返回主選單 (Return)

Business Rules

BR-027

The system SHALL allow duplicate names only after the user confirms with `繼續`.

BR-031

While waiting for confirmation, the system SHALL keep the pending Save request and SHALL NOT store it until the user enters `繼續`.

BR-032

When the user enters `返回主選單` during confirmation, the system SHALL discard the pending Save request and return the user to the Main Menu.

BR-033

When the user enters any input other than `繼續` or `返回主選單` during confirmation, the system SHALL ask the user again to enter `繼續` or `返回主選單` and SHALL remain in the confirmation state.

Acceptance Criteria

AC-016-01

Given an item with the submitted name already exists
When the user submits a valid Save request
Then the system SHALL display a duplicate-name warning and ask the user to enter 繼續 or 返回主選單.

AC-016-02

Given the system is waiting for duplicate-name confirmation
When the user enters 繼續
Then the system SHALL save the item and notify the user that the item was successfully saved (the conversation returns to Idle).

AC-016-03

Given the system is waiting for duplicate-name confirmation
When the user enters 返回主選單
Then the system SHALL NOT save the item, SHALL notify the user that saving was cancelled and return the user to the Main Menu.

AC-016-04

Given the system is waiting for duplicate-name confirmation
When the user enters any input other than 繼續 or 返回主選單
Then the system SHALL ask the user again to enter 繼續 or 返回主選單.

AC-016-05

Given no item with the submitted name exists
When the user submits a valid Save request
Then the system SHALL save the item without displaying a duplicate-name warning.

FR-017 — Switch Destination

The system SHALL allow the user to switch the current destination by entering `切換地區` (alias `切換目的地`) in the Main Menu. The system SHALL list the destinations already stored in the database and accept either the list number or a free-text new destination name. The default destination is `沖繩`.

Business Rules

BR-037

The current destination SHALL be shown in the welcome message and in the Save, Query, Edit and Delete prompts.

BR-038

Save, Query, Edit, Delete, category listing and duplicate-name checks SHALL be limited to the current destination.

BR-039

The current destination SHALL be preserved when the conversation is reset or returned to the Main Menu.

Acceptance Criteria

AC-017-01

Given the user is in the Main Menu
When the user enters 切換地區
Then the system SHALL display the current destination, the stored destinations (numbered) and ask for a number or a new name.

AC-017-02

Given the user is in the Switch Destination flow
When the user enters a valid list number or a new name
Then the system SHALL set the current destination, confirm the change and return to the Main Menu.

FR-018 — Flow Timeout

The system SHALL reset any non-Idle conversation that has been inactive for more than 1 hour.

Business Rules

BR-040

When a timed-out conversation receives `旅遊小幫手`, the system SHALL reset the flow and immediately return to the Main Menu with the previous destination preserved.

BR-041

When a timed-out conversation receives any other message, the system SHALL reset the flow and reply `先前的操作已逾時並已重置，請輸入「旅遊小幫手」開始。`.

FR-019 — LINE Webhook

The system SHALL expose `POST /api/LineWebhook` to receive LINE Messaging API events.

Business Rules

BR-042

The system SHALL verify the `X-Line-Signature` header (HMAC-SHA256 with the channel secret) and respond with HTTP 401 if invalid.

BR-043

The system SHALL process only text message events and SHALL reply through the LINE reply API. Empty responses (e.g. messages received while Idle) SHALL NOT be replied to.

## 7. Non-Functional Requirements

NFR-001 — State Persistence
The system SHALL maintain user conversation states across restarts by utilizing a distributed cache (Redis). Conversation state (including the current destination) SHALL expire after 1 day of inactivity (sliding expiration).

NFR-002 — Data Persistence
The system SHALL ensure that SQLite database files and Redis states survive container restarts by using persistent volumes.

NFR-003 — Database Migration
The system SHALL automatically apply pending EF Core migrations at startup.

NFR-004 — Configuration
The LINE channel secret and access token, SQLite and Redis connection strings SHALL be provided through configuration (`appsettings.json` or environment variables).

## 8. Constraints

CON-001 — Framework
The application SHALL be built on .NET 9.

CON-002 — Deployment
The application SHALL be containerized and orchestrated using Docker Compose.

CON-003 — Database
The primary relational data store SHALL be SQLite.

CON-004 — Messaging Platform
The system SHALL integrate with the LINE Messaging API (webhook and reply API).

## 9. Future Enhancements

- Attach a complete valid input example to Save error messages (FR-012).
- Display the numbered item list when entering the Edit and Delete flows.
- Optional category whitelist or suggestions.
- Keyword search across categories and fields.
- LINE Flex Message / Quick Reply for queries and confirmations; pagination for long results.
- Fetch page title (Open Graph) to prefill the name.
- LINE group chat support and per-user data ownership.
- Export saved items; notes/tags fields.
- Automated deployment to cloud platforms (e.g. NorthFlank).