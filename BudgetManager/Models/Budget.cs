using BudgetManager.Data;

namespace BudgetManager.Models
{
    public class Budget
    {
        public int Id { get; set; }

        // Month and year when the budget starts.
        public int Month { get; set; }

        public int Year { get; set; }

        public decimal LimitAmount { get; set; }

        // If true, the budget applies to future months.
        public bool IsRecurring { get; set; } = false;

        // Exclusive end date for recurring budgets.
        // Null means the budget has no end date.
        public DateTime? EndDate { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser? ApplicationUser { get; set; }
    }
}
