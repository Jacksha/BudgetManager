using BudgetManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BudgetManager.Data
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Category> Categories { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Budget> Budgets { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // CATEGORY

            builder.Entity<Category>()
                .Property(c => c.Name)
                .HasMaxLength(30)
                .IsRequired();

            builder.Entity<Category>()
                .HasOne(c => c.ApplicationUser)
                .WithMany()
                .HasForeignKey(c => c.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TRANSACTION

            builder.Entity<Transaction>()
                .Property(t => t.Description)
                .HasMaxLength(250)
                .IsRequired();

            builder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Transaction>()
                .Property(t => t.Currency)
                .HasMaxLength(3)
                .IsRequired();

            builder.Entity<Transaction>()
                .Property(t => t.ExchangeRate)
                .HasPrecision(18, 6);

            builder.Entity<Transaction>()
                .Property(t => t.AmountInBaseCurrency)
                .HasPrecision(18, 2);

            builder.Entity<Transaction>()
                .HasOne(t => t.ApplicationUser)
                .WithMany()
                .HasForeignKey(t => t.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany()
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // BUDGET

            builder.Entity<Budget>()
                .Property(b => b.LimitAmount)
                .HasPrecision(18, 2);

            builder.Entity<Budget>()
                .HasOne(b => b.ApplicationUser)
                .WithMany()
                .HasForeignKey(b => b.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Budget>()
                .HasOne(b => b.Category)
                .WithMany()
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // APPLICATION USER

            builder.Entity<ApplicationUser>()
                .Property(u => u.BaseCurrency)
                .HasMaxLength(3)
                .IsRequired();

            // Prevent duplicate monthly budgets for the same category.

            builder.Entity<Budget>()
                .HasIndex(b => new
                {
                    b.ApplicationUserId,
                    b.CategoryId,
                    b.Month,
                    b.Year
                })
                .IsUnique();
        }
    }
}
