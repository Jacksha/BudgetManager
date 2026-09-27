using Microsoft.AspNetCore.Identity;

namespace BudgetManager.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string BaseCurrency { get; set; } = "EUR";

        // Name displayed in the application interface.
        // Authentication continues to use the email address.
        public string? DisplayName { get; set; }
    }
}