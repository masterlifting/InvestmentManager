using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.ViewModels.ReportModels.BrokerReportModels
{
    public class DividendErrorForm
    {
        public IEnumerable<ViewModelBase> Companies { get; set; } = new List<ViewModelBase>();
    }
    public class DividendErrorResultModel
    {
        [StringLength(100, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string IdentifierName { get; set; }
        [StringLength(5, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string CompanyId { get; set; }
    }
}
