using InvestmentManager.Entities.Broker;
using System;
using System.Collections.Generic;

namespace InvestmentManager.Web.Models.PortfolioModels
{
    public class PortfolioReportModel
    {
        public PortfolioReportModel()
        {
            Comissions = new List<Comission>();
            StockTransactions = new List<StockTransaction>();
            Dividends = new List<Dividend>();
            AccountTransactions = new List<AccountTransaction>();
            ExchangeRates = new List<ExchangeRate>();
        }

        public string AccountName { get; set; }
        public DateTime DateBeginReport { get; set; }
        public DateTime DateEndReport { get; set; }

        public IEnumerable<ExchangeRate> ExchangeRates { get; set; }
        public IEnumerable<Dividend> Dividends { get; set; }
        public IEnumerable<Comission> Comissions { get; set; }
        public IEnumerable<StockTransaction> StockTransactions { get; set; }
        public IEnumerable<AccountTransaction> AccountTransactions { get; set; }
    }
}
