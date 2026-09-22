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
    }
}