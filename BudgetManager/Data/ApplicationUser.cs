using Microsoft.AspNetCore.Identity;

namespace BudgetManager.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string BaseCurrency { get; set; } = "EUR";
    }
}