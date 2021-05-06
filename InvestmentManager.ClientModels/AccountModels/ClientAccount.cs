using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.ClientModels.AccountModels
{
    public class ClientAccount : ClientBase
    {
        [StringLength(50, ErrorMessage = "max length", MinimumLength = 20), Required(ErrorMessage = "required")]
        public override string Name { get; set; }
        public decimal Sum { get; set; }
    }
}

