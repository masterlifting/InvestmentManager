using System.Collections.Generic;

namespace InvestmentManager.ViewModels.ReportModels.BrokerReportModels
{
    public class BrokerReportModel
    {
        public IList<CorrectBrokerReport> CorrectReports { get; set; } = new List<CorrectBrokerReport>();
        public IList<BrokerReportError> ReportErrors { get; set; } = new List<BrokerReportError>();
    }
    public class CorrectBrokerReport
    {
        public string AccountId { get; set; } = DefaultData.loading;

        public IList<BrokerComission> Comissions { get; set; } = new List<BrokerComission>();
        public IList<BrokerStockTransaction> StockTransactions { get; set; } = new List<BrokerStockTransaction>();
        public IList<BrokerDividend> Dividends { get; set; } = new List<BrokerDividend>();
        public IList<BrokerAccountTransaction> AccountTransactions { get; set; } = new List<BrokerAccountTransaction>();
        public IList<BrokerExchangeRate> ExchangeRates { get; set; } = new List<BrokerExchangeRate>();
    }
    public abstract class BaseBrokerReport
    {
        public string DateOperation { get; set; } = DefaultData.loading;
    }
    public class BrokerComission : BaseBrokerReport
    {
        public string Type { get; set; } = DefaultData.loading;
        public string Amount { get; set; } = DefaultData.loading;
    }
    public class BrokerStockTransaction : BaseBrokerReport
    {
        public string Company { get; set; } = DefaultData.loading;
        public string Ticker { get; set; } = DefaultData.loading;
        public string Quantity { get; set; } = DefaultData.loading;
        public string Cost { get; set; } = DefaultData.loading;
        public string Status { get; set; } = DefaultData.loading;
        public string Exchange { get; set; } = DefaultData.loading;
    }
    public class BrokerDividend : BaseBrokerReport
    {
        public string Company { get; set; } = DefaultData.loading;
        public string Amount { get; set; } = DefaultData.loading;
    }
    public class BrokerAccountTransaction : BaseBrokerReport
    {
        public string Amount { get; set; } = DefaultData.loading;
        public string Status { get; set; } = DefaultData.loading;
    }
    public class BrokerExchangeRate : BaseBrokerReport
    {
        public string Quantity { get; set; } = DefaultData.loading;
        public string Rate { get; set; } = DefaultData.loading;
        public string Status { get; set; } = DefaultData.loading;
    }
    public class BrokerReportError
    {
        public BrokerReportErrorTypes ErrorType { get; set; }
        public string ErrorValue { get; set; }
    }
    public enum BrokerReportErrorTypes
    {
        AccountError,
        AccountTransactionError,
        StockTransactionError,
        ComissionError,
        DividendError,
        ExchangeRateError,
        UndefinedError
    }
}
