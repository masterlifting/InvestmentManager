using InvestmentManager.ViewModels.EntityViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.ViewModels.ReportModels.BrokerReportModels
{
    public class StockTransactionErrorForm
    {
        public IList<ViewModelBase> Companies { get; set; } = new List<ViewModelBase>();
        public IList<LotModel> Lots { get; set; } = new List<LotModel>();
        public IList<ViewModelBase> Exchanges { get; set; } = new List<ViewModelBase>();
    }
    public class StockTransactionErrorResultModel
    {
        [StringLength(10, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string TikerName { get; set; }
        [StringLength(5, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string CompanyId { get; set; }
        [StringLength(5, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string LotId { get; set; }
        [StringLength(5, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string ExchangeId { get; set; }
    }
}
