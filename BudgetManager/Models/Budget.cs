using BudgetManager.Data;

namespace BudgetManager.Models
{
    public class Budget
    {
        public int Id { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public decimal LimitAmount { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser? ApplicationUser { get; set; }
    }
}
