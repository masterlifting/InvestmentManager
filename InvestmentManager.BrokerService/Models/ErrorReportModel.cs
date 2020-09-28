namespace InvestmentManager.BrokerService.Models
{
    public class ErrorReportModel
    {
        public ParseErrorTypes ErrorType { get; set; }
        public string ErrorValue { get; set; }
    }
    public enum ParseErrorTypes
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
