namespace InvestmentManager.Models
{
    public static class Enums
    {
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
        public enum TransactionStatusTypes
        {
            Add = 1,
            Withdraw = 2,
            Buy = 3,
            Sell = 4
        }
        public enum CurrencyTypes
        {
            usd = 1,
            rub = 2
        }
        public enum DataBaseType
        {
            Postgres,
            SQL
        }
    }
}
