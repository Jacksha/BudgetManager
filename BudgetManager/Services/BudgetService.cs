using BudgetManager.Data;
using BudgetManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetManager.Services
{
    public class BudgetService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public BudgetService(
            IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Budget>> GetBudgetsAsync(
            string userId,
            int year,
            int month)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("Invalid user.");

            if (year < 1 || year > 9998 ||
                month < 1 || month > 12)
            {
                throw new ArgumentException("Invalid month or year.");
            }

            var selectedMonth = new DateTime(year, month, 1);

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var applicableBudgets = await context.Budgets
                .AsNoTracking()
                .Include(b => b.Category)
                .Where(b =>
                    b.ApplicationUserId == userId &&

                    // Budget must have started by the selected month.
                    (
                        b.Year < year ||
                        (b.Year == year && b.Month <= month)
                    ) &&

                    // One-time budgets apply only to their own month.
                    (
                        (b.IsRecurring &&
                            (b.EndDate == null ||
                                b.EndDate > selectedMonth)) ||

                        (!b.IsRecurring &&
                            b.Year == year &&
                            b.Month == month)
                    )
                )
                .ToListAsync();

            return applicableBudgets
                .GroupBy(b => b.CategoryId)
                .Select(g => g
                    .OrderByDescending(b => b.Year)
                    .ThenByDescending(b => b.Month)
                    .First())
                .OrderBy(b => b.Category?.Name)
                .ToList();
        }

        public async Task AddBudgetAsync(
            int categoryId,
            decimal limitAmount,
            int year,
            int month,
            string userId,
            bool isRecurring)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("Invalid user.");

            if (limitAmount <= 0)
                throw new ArgumentException(
                    "Budget limit must be greater than zero.");

            if (decimal.Round(limitAmount, 2) != limitAmount)
                throw new ArgumentException(
                    "Budget limit cannot have more than two decimal places.");

            if (year < 1 || year > 9998 ||
                month < 1 || month > 12)
            {
                throw new ArgumentException(
                    "Invalid month or year.");
            }

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var categoryExists = await context.Categories
                .AnyAsync(c =>
                    c.Id == categoryId &&
                    c.ApplicationUserId == userId);

            if (!categoryExists)
                throw new ArgumentException(
                    "Selected category does not exist.");

            var selectedMonth = new DateTime(year, month, 1);

            var budgetExists = await context.Budgets
                .AnyAsync(b =>
                    b.ApplicationUserId == userId &&
                    b.CategoryId == categoryId &&

                    (
                        b.Year < year ||
                        (b.Year == year && b.Month <= month)
                    ) &&

                    (
                        (b.IsRecurring &&
                            (b.EndDate == null ||
                             b.EndDate > selectedMonth)) ||

                        (!b.IsRecurring &&
                            b.Year == year &&
                            b.Month == month)
                    )
                );

            if (budgetExists)
                throw new InvalidOperationException(
                    "A budget for this category already exists for the selected month.");

            var budget = new Budget
            {
                CategoryId = categoryId,
                LimitAmount = limitAmount,
                Year = year,
                Month = month,
                IsRecurring = isRecurring,
                EndDate = null,
                ApplicationUserId = userId
            };

            context.Budgets.Add(budget);

            await context.SaveChangesAsync();
        }


        public async Task UpdateBudgetAsync(
            int budgetId,
            decimal limitAmount,
            int year,
            int month,
            string userId,
            bool isRecurring)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("Invalid user.");

            if (limitAmount <= 0 ||
                decimal.Round(limitAmount, 2) != limitAmount)
            {
                throw new ArgumentException(
                    "Budget limit must be greater than zero and have at most two decimal places.");
            }

            if (year < 1 || year > 9998 ||
                month < 1 || month > 12)
            {
                throw new ArgumentException("Invalid month or year.");
            }

            var selectedMonth = new DateTime(year, month, 1);

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var budget = await context.Budgets
                .FirstOrDefaultAsync(b =>
                    b.Id == budgetId &&
                    b.ApplicationUserId == userId);

            if (budget is null)
                throw new InvalidOperationException("Budget not found.");

            var startMonth = new DateTime(
                budget.Year,
                budget.Month,
                1);

            // Verify that the budget applies to the selected month.
            if (startMonth > selectedMonth ||
                (budget.IsRecurring &&
                    budget.EndDate != null &&
                    budget.EndDate <= selectedMonth) ||
                (!budget.IsRecurring && startMonth != selectedMonth))
            {
                throw new InvalidOperationException(
                    "Budget is not applicable to the selected month.");
            }

            // Do not silently overwrite or reactivate scheduled future rules
            // when changing the recurrence type.
            var hasFutureRules = await context.Budgets
                .AnyAsync(b =>
                    b.ApplicationUserId == userId &&
                    b.CategoryId == budget.CategoryId &&
                    (
                        b.Year > year ||
                        (b.Year == year && b.Month > month)
                    ));

            if (hasFutureRules && isRecurring != budget.IsRecurring)
            {
                throw new InvalidOperationException(
                    "This category has scheduled future budgets. Resolve those rules before changing recurrence.");
            }

            // CASE 1: The rule starts in the selected month.
            if (startMonth == selectedMonth)
            {
                budget.LimitAmount = limitAmount;
                budget.IsRecurring = isRecurring;

                if (!isRecurring)
                {
                    // One-time budgets apply only to their own month.
                    budget.EndDate = null;
                }

                await context.SaveChangesAsync();
                return;
            }

            // CASE 2: We are editing an inherited recurring budget.
            var existingBudget = await context.Budgets
                .AnyAsync(b =>
                    b.ApplicationUserId == userId &&
                    b.CategoryId == budget.CategoryId &&
                    b.Year == year &&
                    b.Month == month);

            if (existingBudget)
            {
                throw new InvalidOperationException(
                    "A budget already starts in the selected month.");
            }

            await using var transaction =
                await context.Database.BeginTransactionAsync();

            // Preserve the previous end date, if one exists.
            var previousEndDate = budget.EndDate;

            // The old rule applies only before the selected month.
            budget.EndDate = selectedMonth;

            // Create the new rule for the selected month.
            context.Budgets.Add(new Budget
            {
                CategoryId = budget.CategoryId,
                LimitAmount = limitAmount,
                Year = year,
                Month = month,
                IsRecurring = isRecurring,
                EndDate = isRecurring ? previousEndDate : null,
                ApplicationUserId = userId
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task DeleteBudgetAsync(
            int budgetId,
            int year,
            int month,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("Invalid user.");

            if (year < 1 || year > 9998 ||
                month < 1 || month > 12)
            {
                throw new ArgumentException("Invalid month or year.");
            }

            var selectedMonth = new DateTime(year, month, 1);

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var budget = await context.Budgets
                .FirstOrDefaultAsync(b =>
                    b.Id == budgetId &&
                    b.ApplicationUserId == userId);

            if (budget is null)
                throw new InvalidOperationException("Budget not found.");

            var startMonth = new DateTime(
                budget.Year,
                budget.Month,
                1);

            // Verify that this budget applies to the selected month.
            if (startMonth > selectedMonth ||
                (budget.IsRecurring &&
                    budget.EndDate != null &&
                    budget.EndDate <= selectedMonth) ||
                (!budget.IsRecurring && startMonth != selectedMonth))
            {
                throw new InvalidOperationException(
                    "Budget is not applicable to the selected month.");
            }

            // A one-time budget can simply be deleted.
            if (!budget.IsRecurring)
            {
                context.Budgets.Remove(budget);

                await context.SaveChangesAsync();
                return;
            }

            // Remove all rules starting from the selected month.
            // This also removes previously scheduled future changes.
            var futureBudgets = await context.Budgets
                .Where(b =>
                    b.ApplicationUserId == userId &&
                    b.CategoryId == budget.CategoryId &&
                    (
                        b.Year > year ||
                        (b.Year == year && b.Month >= month)
                    ))
                .ToListAsync();

            context.Budgets.RemoveRange(futureBudgets);

            // Preserve previous months if the recurring rule
            // started before the selected month.
            if (startMonth < selectedMonth)
            {
                budget.EndDate = selectedMonth;
            }

            await context.SaveChangesAsync();
        }
    }
}
