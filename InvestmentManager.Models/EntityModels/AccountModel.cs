using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Models.EntityModels
{
    public class AccountModel : BaseView
    {
        [StringLength(50, ErrorMessage = ErrorMessages.errorLenght, MinimumLength = 20), Required(ErrorMessage = ErrorMessages.errorRequired)]
        public override string Name { get; set; }
    }
}
