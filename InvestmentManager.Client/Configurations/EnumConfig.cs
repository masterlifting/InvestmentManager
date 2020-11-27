namespace InvestmentManager.Client.Configurations
{
    public static class EnumConfig
    {
        public enum TransactionStatus
        {
            None,
            Buy,
            Sell,
            Add,
            Withdraw
        }
        public enum Exchange
        {
            MMVB,
            SPB
        }
        public enum Currency
        {
            usd,
            rub
        }
        public enum UrlController
        {
            Accounts,
            AccountTransactions,
            BuyRecommendations,
            Comissions,
            Companies,
            Dividends,
            ExchangeRates,
            Isins,
            Prices,
            Reports,
            ReportSources,
            SellRecommendations,
            Services,
            StockTransactions,
            Tickers,
            Sectors,
            Industries
        }
        public enum UrlPath
        {
            None,
            ByCompanyId,
            ByAccountIds
        }
        public enum UrlOption
        {
            None,
            New,
            Last,
            Pagination,
            OrderBy,
            OrderDesc,
            Summary
        }
        public enum UrlCatalog
        {
            CurrencyTypes,
            ExchangeTypes,
            ComissionTypes,
            StatusTypes,
            LotTypes
        }
        public enum UrlService
        {
            RecalculateAll,
            ParseBrokerReports,
            ParseReports,
            ParsePrices,
            Rate
        }
        public enum ColorCustom
        {
            isuccess,
            idanger,
            iwarning,
            iinfo,
            isecondary,
            idark
        }
        public enum ColorBootstrap
        {
            light,
            success,
            danger,
            primary,
            warning,
            info,
            secondary,
            dark,
            muted
        }
        public enum HtmlDataType
        {
            NotSet,
            String,
            Number,
            Date,
            Boolean,
            Currency
        }
        public enum AlignType
        {
            Left,
            Right,
            Center
        }
    }
}
