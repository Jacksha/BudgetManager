# Budget Manager

A personal finance web application developed as a final thesis project at VSITE, Zagreb.

**Status:** Deployed and functional; documentation and further testing in progress.  
**Live application:** https://jacksha-001-site1.ctempurl.com/

## Overview

Budget Manager lets registered users track income and expenses, organize transactions into categories, define monthly spending limits, and review their finances on an interactive dashboard. Each user's financial data is isolated from other accounts.

## Technology Stack

- C# / .NET 8 and Blazor Web App (Interactive Server)
- ASP.NET Core Identity
- Entity Framework Core and Microsoft SQL Server
- MailKit and Brevo SMTP for transactional email
- Bootstrap, HTML, CSS, and JavaScript interop
- IIS / ASP.NET Core Module on SmarterASP.NET

## Features

### Accounts

- Registration and sign-in through ASP.NET Core Identity
- Email confirmation required before sign-in
- Confirmation and password-reset email sending via SMTP
- Optional user display name
- User-specific financial records and protected application pages

### Categories and Transactions

- Create, edit, and delete categories
- Record income and expenses with amount, date, category, and optional description
- Unified Add/Edit transaction form and confirmation before deletion
- Monthly navigation, filtering by category and transaction type, and description search

### Budgets

- Monthly spending limits per category
- One-time and recurring ("Every month") budgets
- Editing and ending recurring budgets while preserving historical values
- Unified Add/Edit form and confirmation before deletion or ending a budget

### Dashboard

- Monthly income, expenses, and balance
- Month-to-month navigation
- Expenses by category with integrated budget status
- Green indicators for spending within limits and red indicators for over-budget amounts
- Remaining or exceeded budget amounts
- Animated donut chart for expense distribution

## Architecture

The application separates Blazor UI components, business logic in services, and EF Core data access. ASP.NET Core Identity manages user accounts. The app uses `IDbContextFactory<ApplicationDbContext>` for database context creation.

Recurring budget rules preserve their historical validity: modifying a recurring budget closes the previous rule and begins a new one. The end date is exclusive.

## Run Locally

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or compatible .NET development environment
- SQL Server or SQL Server LocalDB
- SMTP account and verified sender address to test email confirmation

1. Clone the repository:

   ```bash
   git clone https://github.com/Jacksha/BudgetManager.git
   ```

2. Open the solution and configure `ConnectionStrings:DefaultConnection` for your local database.
3. Apply EF Core migrations, for example in Package Manager Console:

   ```powershell
   Update-Database
   ```

4. Configure SMTP settings. The application reads `Smtp:Host`, `Smtp:Port`, `Smtp:FromEmail`, `Smtp:Username`, and `Smtp:Password`. The sender must be verified with the chosen provider. For Brevo, use `smtp-relay.brevo.com` on port `587` with STARTTLS.
5. Store the SMTP login and key in **.NET User Secrets**, not in source-controlled files. In Visual Studio, right-click the project and select **Manage User Secrets**:

   ```json
   {
     "Smtp": {
       "Username": "YOUR_SMTP_LOGIN",
       "Password": "YOUR_SMTP_KEY"
     }
   }
   ```

6. Run the application. Registration confirmation requires a working SMTP configuration.

**Never commit real SQL credentials, SMTP keys, or production configuration.**

## Deployment

The application is deployed to SmarterASP.NET via Visual Studio Web Deploy, using a hosted SQL Server database.

- The production database schema was created using an EF Core migration SQL script.
- Production SQL and SMTP credentials are configured in the server-only `appsettings.Production.json` file, which must remain outside source control.
- A root-level `web.config` in the project preserves the hosting-specific IIS settings (`anonymousAuthentication` enabled, `basicAuthentication` disabled) during publication.
- Keep **Remove additional files at destination** unchecked and verify the server-only production configuration after publishing.
- Leave the Publish profile's **Use this connection string at runtime** options unchecked; the application reads its production configuration instead.

Registration, email confirmation, and sign-in have been tested successfully on the deployed application.

## Further Work

- Additional validation and automated tests
- Further responsive-layout and usability improvements
- Additional financial reports and statistics

## Author

**Dario Jakovljević**  
Final thesis project — VSITE, Zagreb.
