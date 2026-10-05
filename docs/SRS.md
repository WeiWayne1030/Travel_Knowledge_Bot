# Okinawa Travel Knowledge Bot
## Software Requirements Specification

## 1. Introduction
Okinawa Travel Knowledge Bot is a personal travel information management system integrated with LINE.

The system allows users to save travel-related URLs through LINE, automatically classify the information using AI, and search previously saved travel information.

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
2. Automatically classify saved travel information.
3. Extract useful metadata from saved URLs.
4. Allow users to search saved information.
5. Organize travel information into structured categories.
6. Reduce the effort required to manually organize travel information.

## 4. Scope
### 4.1 In Scope

The MVP includes:

- Receiving messages from LINE.
- Detecting URLs in messages.
- Extracting basic information from URLs.
- Automatically classifying travel information.
- Allowing users to specify a category manually.
- Storing travel information.
- Searching saved travel information.
- Viewing saved travel information.

### 4.2 Out of Scope

The MVP does not include:

- Automatic itinerary generation.
- Hotel or restaurant reservations.
- Navigation.
- Real-time travel recommendations.
- Multi-user collaboration.
- Mobile applications.
- A dedicated web frontend.
- Advanced recommendation algorithms.
## 5. Users
### 5.1 Personal User

The system is initially designed for a single personal user who collects and organizes travel information through LINE.

## 6. Functional Requirements
FR-001 — Bot Activation and Main Menu

The system SHALL provide a bot activation mechanism. When the user activates the bot, the system SHALL display a welcome message and the available commands.

Available commands:
-Save
-Query
-Edit
-Delete

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
Then the system SHALL provide the available commands: Save, Query, Edit and Delete.

FR-002 — Command Selection

The system SHALL allow the user to select an operation from the Main Menu by entering one of the supported commands: Save, Query, Edit, or Delete.

The user SHALL be able to select one of these functions.

Business Rules

<!-- 第一版先只支援增刪改查-->
BR-001

The system SHALL support the Save, Query, Edit and Delete functions in the initial version.

BR-002

The system SHALL execute the flow corresponding to the function selected by the user.
Acceptance Criteria

AC-002-01

Given the user has activated the bot
When the user selects Save
Then the system SHALL enter the Save flow.

AC-002-02

Given the user has activated the bot
When the user selects Query
Then the system SHALL enter the Query flow.

AC-002-03

Given the user has activated the bot
When the user selects Edit
Then the system SHALL enter the Edit flow.

AC-002-04

Given the user has activated the bot
When the user selects Delete
Then the system SHALL enter the Delete flow.

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

The system SHALL allow the user to enter `Return` from any interactive flow and return to the Main Menu.

Business Rules

BR-003

The `Return` command SHALL be available in all interactive states except the Main Menu.

BR-004

When the system receives `Return`, it SHALL discard the current operation state and return the user to the Main Menu.

Acceptance Criteria

AC-004-01

Given the user is in the Save flow
When the user enters `Return`
Then the system SHALL return the user to the Main Menu.

AC-004-02

Given the user is in the Query flow
When the user enters `Return`
Then the system SHALL return the user to the Main Menu.

AC-004-03

Given the user is in the Edit flow
When the user enters `Return`
Then the system SHALL return the user to the Main Menu.

AC-004-04

Given the user is in the Delete flow
When the user enters `Return`
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
Then the system SHALL notify the user that the item was successfully saved.

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

When the user selects the Query function, the system SHALL display the available categories that can be queried.

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

The displayed categories SHALL be limited to categories supported by the system.

BR-014

The Query flow SHALL only allow the user to query supported categories.
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

The returned information SHALL include the stored name, category and corresponding URL.

Business Rules

BR-015

The system SHALL only return items matching the selected category.

BR-016

The system SHALL return the name and URL associated with each matching item.

BR-017

If no items exist for the selected category, the system SHALL notify the user that no matching items were found.
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
Then the system SHALL return all matching items according to the system's result display rules.

FR-010 — Missing URL Handling

If the user submits an input without a valid URL, the system SHALL display an error message and provide the expected input format.

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
Then the system SHALL provide an example of the expected URL #Category Name input format.

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
Then the system SHALL provide an example of a valid input format.

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

The system SHALL allow the user to select a saved item and update its URL, name or category.

Business Rules

BR-023

The system SHALL identify the saved item selected by the user from the query results.

BR-024

The system SHALL validate updated values using the same requirements as a Save request before storing the changes.

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

The system SHALL request confirmation before permanently deleting a saved item.

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
Then the system SHALL keep the item and notify the user that deletion was cancelled.

FR-016 — Duplicate Name Confirmation

When the user submits a Save request with a name that matches an existing saved item, the system SHALL warn the user that the name is already in use and ask the user to confirm whether to continue saving.

The user SHALL respond with one of the following commands:
-Continue
-Return

Business Rules

BR-027

The system SHALL allow duplicate names only after the user confirms with `Continue`.

BR-031

While waiting for confirmation, the system SHALL keep the pending Save request and SHALL NOT store it until the user enters `Continue`.

BR-032

When the user enters `Return` during confirmation, the system SHALL discard the pending Save request and return the user to the Main Menu.

BR-033

When the user enters any input other than `Continue` or `Return` during confirmation, the system SHALL ask the user again to enter `Continue` or `Return` and SHALL remain in the confirmation state.

Acceptance Criteria

AC-016-01

Given an item with the submitted name already exists
When the user submits a valid Save request
Then the system SHALL display a duplicate-name warning and ask the user to enter Continue or Return.

AC-016-02

Given the system is waiting for duplicate-name confirmation
When the user enters Continue
Then the system SHALL save the item, notify the user that the item was successfully saved and return the user to the Main Menu.

AC-016-03

Given the system is waiting for duplicate-name confirmation
When the user enters Return
Then the system SHALL NOT save the item, SHALL notify the user that saving was cancelled and return the user to the Main Menu.

AC-016-04

Given the system is waiting for duplicate-name confirmation
When the user enters any input other than Continue or Return
Then the system SHALL ask the user again to enter Continue or Return.

AC-016-05

Given no item with the submitted name exists
When the user submits a valid Save request
Then the system SHALL save the item without displaying a duplicate-name warning.

## 7. Non-Functional Requirements

## 8. Constraints

## 9. Future Enhancements