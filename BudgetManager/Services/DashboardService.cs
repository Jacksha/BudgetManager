
using BudgetManager.Data;
using BudgetManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetManager.Services
{
    public class DashboardService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public DashboardService(
            IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<DashboardSummary> GetMonthlySummaryAsync(
            string userId,
            int year,
            int month)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("Invalid user.");

            if (year < 1 || year > 9998 || month < 1 || month > 12)
                throw new ArgumentException("Invalid month or year.");

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var monthlyTransactions = context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.ApplicationUserId == userId &&
                    t.Date >= startDate &&
                    t.Date < endDate);

            var income = await monthlyTransactions
                .Where(t => t.Type == TransactionType.Income)
                .SumAsync(t => (decimal?)t.AmountInBaseCurrency) ?? 0m;

            var expenses = await monthlyTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .SumAsync(t => (decimal?)t.AmountInBaseCurrency) ?? 0m;

            return new DashboardSummary
            {
                TotalIncome = income,
                TotalExpenses = expenses,
                Balance = income - expenses
            };
        }

        public async Task<List<CategoryExpenseSummary>> GetExpensesByCategoryAsync(
            string userId,
            int year,
            int month)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("Invalid user.");

            if (year < 1 || year > 9998 || month < 1 || month > 12)
                throw new ArgumentException("Invalid month or year.");

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            return await context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.ApplicationUserId == userId &&
                    t.Type == TransactionType.Expense &&
                    t.Date >= startDate &&
                    t.Date < endDate)
                .GroupBy(t => new
                {
                    t.CategoryId,
                    CategoryName = t.Category!.Name
                })
                .Select(g => new CategoryExpenseSummary
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    TotalAmount = g.Sum(t => t.AmountInBaseCurrency)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToListAsync();
        }
    }

    public class DashboardSummary
    {
        public decimal TotalIncome { get; set; }

        public decimal TotalExpenses { get; set; }

        public decimal Balance { get; set; }
    }

    public class CategoryExpenseSummary
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }
    }

}