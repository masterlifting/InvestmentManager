using System;

namespace InvestmentManager.Web.Models.PortfolioModels
{
    public class StockOperationModel
    {
        public long CompanyId { get; set; }
        public string CompanyName { get; set; }
        public DateTime DateLastOperation { get; set; }
        public int FreeLot { get; set; }
        public decimal Profit { get; set; }
        public string CurrencyType { get; set; }
    }
}
