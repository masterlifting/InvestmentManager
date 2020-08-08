using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Web.Models.PortfolioModels
{
    public class AccountTransactionErrorForm
    {
        [Display(Name = "Введи название нового соглашения")]
        [Required]
        [StringLength(30, MinimumLength = 1)]
        public string AccountName { get; set; }
    }
}
