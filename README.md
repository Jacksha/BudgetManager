
# Budget Manager

A web application for personal finance management, developed as part of a final thesis project at VSITE, Zagreb.

**Development status:** Active development

## Overview

Budget Manager helps users manage their personal finances by tracking income and expenses, organizing transactions into categories, and defining monthly spending limits.

The application includes an interactive financial dashboard, budget tracking, and visual spending analysis.

Each user has access only to their own financial data.

## Technologies

- C# / .NET 8
- Blazor Web App (Interactive Server)
- ASP.NET Core Identity
- Entity Framework Core
- Microsoft SQL Server
- Bootstrap
- HTML and CSS
- JavaScript interop

## Implemented Features

### User Management

- User registration and authentication
- ASP.NET Core Identity integration
- User-specific financial data
- Protection of authenticated application pages

### Categories

- Create, edit, and delete financial categories
- Organize transactions and budgets by category
- User-specific category management

### Transactions

- Record income and expenses
- Create, edit, and delete transactions
- Specify transaction amount, date, category, and description
- Unified Add/Edit form
- Automatic scrolling to the editing form
- Confirmation before deletion

### Monthly Budgets

- Define monthly spending limits for individual categories
- Create one-time or recurring budgets
- Automatically apply recurring budgets to future months
- Modify budget limits without losing historical values
- Stop recurring budgets while preserving previous months
- Track budget validity using start month and end date
- Unified Add/Edit form
- Confirmation before deleting or stopping budgets

### Financial Dashboard

- Monthly income overview
- Monthly expense overview
- Balance calculation
- Navigation between months
- Spending breakdown by category
- Visual expense distribution using an interactive donut chart
- Animated progress bars

### Budget Tracking

The dashboard integrates budget information directly into the expense breakdown by category.

- Categories without a budget display their share of total expenses.
- Categories with a budget use green progress bars for spending within the limit.
- Spending exceeding the budget is highlighted in red.
- Remaining budget and exceeded amounts are displayed.
- Categories with a defined budget remain visible even when no expenses have been recorded.

Progress bar lengths represent each category's share of total monthly expenses.

## Application Architecture

The application uses a layered structure that separates the user interface, business logic, and data access.

- **Components:** Blazor pages, reusable UI elements, and application layout.
- **Models:** Application entities such as transactions, categories, and budgets.
- **Services:** Business logic, validation, and database operations.
- **Data:** Entity Framework Core database context, ASP.NET Core Identity, and database migrations.

Entity Framework Core is used for database operations, while ASP.NET Core Identity manages user accounts and authentication.

## Budget Management Logic

Recurring budgets are represented using a starting month, year, and optional end date.

When a recurring budget is modified in a later month, the previous rule is closed and a new rule is created. This preserves historical budget values and allows the application to display the correct limit for each month.

The end date is exclusive, meaning the previous budget rule no longer applies from that date onward.

## Planned Features

- Transaction filtering by month, category, and type
- Transaction search
- Additional financial statistics
- Exchange-rate REST API integration
- Additional validation and testing
- Deployment to a web-hosting or cloud environment

## Running the Project Locally

### Prerequisites

- .NET 8 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 or a compatible development environment

### Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/Jacksha/BudgetManager.git
   ```

2. Open the solution in Visual Studio.

3. Configure the `DefaultConnection` connection string for your local SQL Server instance.

4. Apply Entity Framework Core migrations using the Package Manager Console:

   ```powershell
   Update-Database
   ```

5. Run the application.

The database schema is managed through Entity Framework Core migrations.

## Future Development

The project will continue to evolve through improved transaction filtering, additional financial analysis, testing, and deployment.

The final goal is to provide a responsive and practical application for tracking personal finances and managing monthly spending.

## Author

**Dario Jakovljević**

Final thesis project – VSITE, Zagreb.
