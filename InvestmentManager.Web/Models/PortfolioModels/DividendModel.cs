using System;

namespace InvestmentManager.Web.Models.PortfolioModels
{
    public class DividendModel
    {
        public string CompanyName { get; set; }
        public decimal DividendSum { get; set; }
        public string CurrencyType { get; set; }
        public DateTime LastDate { get; set; }
        public int PayCount { get; set; }
    }
}
