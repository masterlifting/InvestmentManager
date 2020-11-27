using static InvestmentManager.Client.Configurations.EnumConfig;
namespace InvestmentManager.Client.Configurations
{
    public static class InvestHelper
    {
        public static TransactionStatus GetTransactionStatus(long transactionId) => transactionId switch
        {
            1 => TransactionStatus.Add,
            2 => TransactionStatus.Withdraw,
            3 => TransactionStatus.Buy,
            4 => TransactionStatus.Sell,
            _ => TransactionStatus.None
        };
        public static Exchange GetExchange(long exchangeId) => exchangeId switch
        {
            1 => Exchange.MMVB,
            2 => Exchange.SPB,
            _ => throw new System.NotImplementedException(),
        };
        public static Currency GetCurrency(long currencyId) => currencyId switch
        {
            1 => Currency.usd,
            2 => Currency.rub,
            _ => throw new System.NotImplementedException(),
        };
    }
}
