using System;
using System.Collections.Generic;

namespace InvestmentManager.Web.Models.PortfolioModels
{
    public class ResultBrokerReportViewModel
    {
        public ResultBrokerReportViewModel()
        {
            Comissions = new List<ComissionViewModel>();
            StockTransactions = new List<StockTransactionViewModel>();
            Dividends = new List<DividendViewModel>();
            AccountTransactions = new List<AccountTransactionViewModel>();
            ExchangeRates = new List<ExchangeRateViewModel>();
        }

        public IEnumerable<ComissionViewModel> Comissions { get; set; }
        public IEnumerable<StockTransactionViewModel> StockTransactions { get; set; }
        public IEnumerable<DividendViewModel> Dividends { get; set; }
        public IEnumerable<AccountTransactionViewModel> AccountTransactions { get; set; }
        public IEnumerable<ExchangeRateViewModel> ExchangeRates { get; set; }
    }
    public class BaseReportViewModel
    {
        public string AccountName { get; set; }
        public DateTime DateOperation { get; set; }
        public string Currency { get; set; }
    }
    public class ComissionViewModel : BaseReportViewModel
    {
        public string ComissionTypeName { get; set; }
        public decimal Amount { get; set; }
    }
    public class StockTransactionViewModel : BaseReportViewModel
    {
        public string CompanyName { get; set; }
        public long Identifier { get; set; }
        public int Quantity { get; set; }
        public decimal Cost { get; set; }
        public string TransactionStatus { get; set; }
        public string Ticker { get; set; }
        public string Exchange { get; set; }
    }
    public class DividendViewModel : BaseReportViewModel
    {
        public string CompanyName { get; set; }
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public string Isin { get; set; }
    }
    public class AccountTransactionViewModel : BaseReportViewModel
    {
        public decimal Amount { get; set; }
        public string TransactionStatus { get; set; }
    }
    public class ExchangeRateViewModel : BaseReportViewModel
    {
        public long Identifier { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public string TransactionStatus { get; set; }
    }
}
