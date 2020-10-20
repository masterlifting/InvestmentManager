using System;

namespace InvestmentManager.ViewModels.CompanyModels
{
    public class StockTransactionModel
    {
        public long CompanyId { get; set; }
        public string CompanyName { get; set; }
        public DateTime DateLastTransaction { get; set; }
        public int FreeLot { get; set; }
        public decimal ProfitCurrent { get; set; }
        public long CurrencyId { get; set; }
    }
}
