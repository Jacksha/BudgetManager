using BudgetManager.Data;

namespace BudgetManager.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "EUR";

        public decimal ExchangeRate { get; set; } = 1m;

        public decimal AmountInBaseCurrency { get; set; }

        public DateTime Date { get; set; } = DateTime.Today;

        public TransactionType Type { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser? ApplicationUser { get; set; }
    }
}
