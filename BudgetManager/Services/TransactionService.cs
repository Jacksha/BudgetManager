using BudgetManager.Data;
using BudgetManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetManager.Services
{
    public class TransactionService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public TransactionService(
            IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Transaction>> GetTransactionsAsync(string userId)
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();

            return await context.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => t.ApplicationUserId == userId)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync();
        }

        public async Task AddTransactionAsync(
            string description,
            decimal amount,
            DateTime date,
            TransactionType type,
            int categoryId,
            string userId)
        {
            // Validate input.

            description = description?.Trim() ?? string.Empty;

            if (description.Length > 250)
                throw new ArgumentException(
                    "Description cannot exceed 250 characters.");

            if (amount <= 0)
                throw new ArgumentException(
                    "Amount must be greater than zero.");

            if (decimal.Round(amount, 2) != amount)
                throw new ArgumentException(
                    "Amount cannot have more than two decimal places.");

            if (!Enum.IsDefined(type))
                throw new ArgumentException(
                    "Invalid transaction type.");

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(
                    "Invalid user.");

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            // Verify that the category belongs to this user.

            var categoryExists = await context.Categories
                .AnyAsync(c =>
                    c.Id == categoryId &&
                    c.ApplicationUserId == userId);

            if (!categoryExists)
                throw new ArgumentException(
                    "Selected category does not exist.");

            // Create the transaction.

            var transaction = new Transaction
            {
                Description = description,
                Amount = amount,
                Currency = "EUR",
                ExchangeRate = 1m,
                AmountInBaseCurrency = amount,
                Date = date.Date,
                Type = type,
                CategoryId = categoryId,
                ApplicationUserId = userId
            };

            context.Transactions.Add(transaction);

            await context.SaveChangesAsync();
        }

        public async Task UpdateTransactionAsync(
            int transactionId,
            string description,
            decimal amount,
            DateTime date,
            TransactionType type,
            int categoryId,
            string userId)
        {
            description = description?.Trim() ?? string.Empty;

            if (description.Length > 250)
                throw new ArgumentException(
                    "Description cannot exceed 250 characters.");

            if (amount <= 0)
                throw new ArgumentException(
                    "Amount must be greater than zero.");

            if (decimal.Round(amount, 2) != amount)
                throw new ArgumentException(
                    "Amount cannot have more than two decimal places.");

            if (!Enum.IsDefined(type))
                throw new ArgumentException(
                    "Invalid transaction type.");

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(
                    "Invalid user.");

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            // Find the transaction belonging to this user.
            var transaction = await context.Transactions
                .FirstOrDefaultAsync(t =>
                    t.Id == transactionId &&
                    t.ApplicationUserId == userId);

            if (transaction is null)
                throw new InvalidOperationException(
                    "Transaction not found.");

            // Verify that the selected category belongs to the user.
            var categoryExists = await context.Categories
                .AnyAsync(c =>
                    c.Id == categoryId &&
                    c.ApplicationUserId == userId);

            if (!categoryExists)
                throw new ArgumentException(
                    "Selected category does not exist.");

            // Update transaction properties.
            transaction.Description = description;
            transaction.Amount = amount;
            transaction.AmountInBaseCurrency = amount;
            transaction.Date = date.Date;
            transaction.Type = type;
            transaction.CategoryId = categoryId;

            await context.SaveChangesAsync();
        }

        public async Task DeleteTransactionAsync(
            int transactionId,
            string userId)
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var transaction = await context.Transactions
                .FirstOrDefaultAsync(t =>
                    t.Id == transactionId &&
                    t.ApplicationUserId == userId);

            if (transaction is null)
                throw new InvalidOperationException(
                    "Transaction not found.");

            context.Transactions.Remove(transaction);

            await context.SaveChangesAsync();
        }
    }
}
