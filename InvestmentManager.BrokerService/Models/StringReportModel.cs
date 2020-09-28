using System;
using System.Collections.Generic;

namespace InvestmentManager.BrokerService.Models
{
    public class StringReportModel
    {
        public string AccountId { get; set; }
        public DateTime DateBeginReport { get; set; }
        public DateTime DateEndReport { get; set; }
        public IEnumerable<StringComissionModel> Comissions { get; set; } = new List<StringComissionModel>();
        public IEnumerable<StringStockTransactionModel> StockTransactions { get; set; } = new List<StringStockTransactionModel>();
        public IEnumerable<StringDividendModel> Dividends { get; set; } = new List<StringDividendModel>();
        public IEnumerable<StringAccountTransactionModel> AccountTransactions { get; set; } = new List<StringAccountTransactionModel>();
        public IEnumerable<StringExchangeRateModel> ExchangeRates { get; set; } = new List<StringExchangeRateModel>();
    }
    public class StringBaseReportField
    {
        public string DateOperation { get; set; }
        public string Currency { get; set; }
    }
    public class StringComissionModel : StringBaseReportField
    {
        public string Type { get; set; }
        public string Amount { get; set; }
    }
    public class StringStockTransactionModel : StringBaseReportField
    {
        public string Identifier { get; set; }
        public string Quantity { get; set; }
        public string Cost { get; set; }
        public string TransactionStatus { get; set; }
        public string Ticker { get; set; }
        public string Exchange { get; set; }
    }
    public class StringDividendModel : StringBaseReportField
    {
        public string CompanyName { get; set; }
        public string Amount { get; set; }
    }
    public class StringAccountTransactionModel : StringBaseReportField
    {
        public string Amount { get; set; }
        public string TransactionStatus { get; set; }
    }
    public class StringExchangeRateModel : StringBaseReportField
    {
        public string Identifier { get; set; }
        public string Quantity { get; set; }
        public string Rate { get; set; }
        public string TransactionStatus { get; set; }
    }
}
