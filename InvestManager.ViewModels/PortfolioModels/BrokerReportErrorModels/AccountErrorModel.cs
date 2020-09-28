using System.ComponentModel.DataAnnotations;

namespace InvestManager.ViewModels.PortfolioModels.BrokerReportErrorModels
{
    public class AccountErrorResultModel
    {
        [StringLength(50, ErrorMessage = DefaultData.errorLenght,MinimumLength =20), Required(ErrorMessage = DefaultData.errorRequired)]
        public string AccountName { get; set; }
    }
}
