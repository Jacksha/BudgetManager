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
            await using var context =
                await _contextFactory.CreateDbContextAsync();

            return await context.Budgets
                .AsNoTracking()
                .Include(b => b.Category)
                .Where(b =>
                    b.ApplicationUserId == userId &&
                    b.Year == year &&
                    b.Month == month)
                .OrderBy(b => b.Category!.Name)
                .ToListAsync();
        }

        public async Task AddBudgetAsync(
            int categoryId,
            decimal limitAmount,
            int year,
            int month,
            string userId)
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

            var budgetExists = await context.Budgets
                .AnyAsync(b =>
                    b.ApplicationUserId == userId &&
                    b.CategoryId == categoryId &&
                    b.Year == year &&
                    b.Month == month);

            if (budgetExists)
                throw new InvalidOperationException(
                    "A budget for this category already exists for the selected month.");

            var budget = new Budget
            {
                CategoryId = categoryId,
                LimitAmount = limitAmount,
                Year = year,
                Month = month,
                ApplicationUserId = userId
            };

            context.Budgets.Add(budget);

            await context.SaveChangesAsync();
        }
    }
}