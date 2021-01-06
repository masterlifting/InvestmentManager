namespace InvestmentManager.Client.Configurations
{
    public static class EnumConfig
    {
        #region Url config
        public enum UrlController
        {
            Accounts,
            AccountTransactions,
            BuyRecommendations,
            Coefficients,
            Comissions,
            ComissionTypes,
            Companies,
            Dividends,
            ExchangeRates,
            Isins,
            Prices,
            Ratings,
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
            ByCompanyId,
            ByAccountId,
            ByPagination
        }
        public enum UrlOption
        {
            New,
            Last,
            Summary,
            Additional
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
            ResetCalculator,
            ResetSummary,
            ParseBrokerReports,
            ParseReports,
            ParsePrices,
            Rate
        }
        #endregion

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
