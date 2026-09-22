using BudgetManager.Data;
using BudgetManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetManager.Services
{
    public class CategoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public CategoryService(
            IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Category>> GetCategoriesAsync(string userId)
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();

            return await context.Categories
                .AsNoTracking()
                .Where(c => c.ApplicationUserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task AddCategoryAsync(string name, string userId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Category name cannot be empty.");
            }

            name = name.Trim();

            if (name.Length > 35)
            {
                throw new ArgumentException(
                    "Category name cannot exceed 35 characters.");
            }

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var category = new Category
            {
                Name = name,
                ApplicationUserId = userId
            };

            context.Categories.Add(category);

            await context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(
            int categoryId,
            string name,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name cannot be empty.");

            name = name.Trim();

            if (name.Length > 35)
                throw new ArgumentException(
                    "Category name cannot exceed 35 characters.");

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var category = await context.Categories
                .FirstOrDefaultAsync(c =>
                    c.Id == categoryId &&
                    c.ApplicationUserId == userId);

            if (category is null)
                throw new InvalidOperationException("Category not found.");

            category.Name = name;

            await context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(
            int categoryId,
            string userId)
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var category = await context.Categories
                .FirstOrDefaultAsync(c =>
                    c.Id == categoryId &&
                    c.ApplicationUserId == userId);

            if (category is null)
                throw new InvalidOperationException("Category not found.");

            var hasTransactions = await context.Transactions
                .AnyAsync(t => t.CategoryId == categoryId);

            var hasBudgets = await context.Budgets
                .AnyAsync(b => b.CategoryId == categoryId);

            if (hasTransactions || hasBudgets)
            {
                throw new InvalidOperationException(
                    "Cannot delete a category that has transactions or budgets.");
            }

            context.Categories.Remove(category);

            await context.SaveChangesAsync();
        }
    }
}