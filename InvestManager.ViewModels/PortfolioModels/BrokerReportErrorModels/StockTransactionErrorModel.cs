using InvestManager.DomainModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestManager.ViewModels.PortfolioModels.BrokerReportErrorModels
{
    public class StockTransactionErrorModel
    {
        public IList<CompanyD> Companies { get; set; } = new List<CompanyD>();
        public IList<LotD> Lots { get; set; } = new List<LotD>();
        public IList<ExchangeD> Exchanges { get; set; } = new List<ExchangeD>();
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
