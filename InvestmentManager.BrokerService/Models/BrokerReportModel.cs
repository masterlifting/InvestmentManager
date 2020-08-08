using System;
using System.Collections.Generic;

namespace InvestmentManager.BrokerService.Models
{
    public class BrokerReportModel
    {
        public BrokerReportModel()
        {
            Comissions = new List<BrockerComissionModel>();
            StockTransactions = new List<BrockerStockTransactionModel>();
            Dividends = new List<BrockerDividendModel>();
            AccountTransactions = new List<BrockerAccountTransactionModel>();
            ExchangeRates = new List<BrockerExchangeRateModel>();
        }

        public DateTime DateBeginReport { get; set; }
        public DateTime DateEndReport { get; set; }
        public string AccountId { get; set; }
        public IEnumerable<BrockerComissionModel> Comissions { get; set; }
        public IEnumerable<BrockerStockTransactionModel> StockTransactions { get; set; }
        public IEnumerable<BrockerDividendModel> Dividends { get; set; }
        public IEnumerable<BrockerAccountTransactionModel> AccountTransactions { get; set; }
        public IEnumerable<BrockerExchangeRateModel> ExchangeRates { get; set; }
    }
    public class BrockerBaseReportField
    {
        public string DateOperation { get; set; }
        public string Currency { get; set; }
    }
    public class BrockerComissionModel : BrockerBaseReportField
    {
        public string Type { get; set; }
        public string Amount { get; set; }
    }
    public class BrockerStockTransactionModel : BrockerBaseReportField
    {
        public string Identifier { get; set; }
        public string Quantity { get; set; }
        public string Cost { get; set; }
        public string TransactionStatus { get; set; }
        public string Ticker { get; set; }
        public string Exchange { get; set; }
    }
    public class BrockerDividendModel : BrockerBaseReportField
    {
        public string CompanyName { get; set; }
        public string Amount { get; set; }
    }
    public class BrockerAccountTransactionModel : BrockerBaseReportField
    {
        public string Amount { get; set; }
        public string TransactionStatus { get; set; }
    }
    public class BrockerExchangeRateModel : BrockerBaseReportField
    {
        public string Identifier { get; set; }
        public string Quantity { get; set; }
        public string Rate { get; set; }
        public string TransactionStatus { get; set; }
    }
}
