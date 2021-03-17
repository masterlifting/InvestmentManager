using System;
using static InvestmentManager.Client.Configurations.EnumConfig;

namespace InvestmentManager.Client.Services.HttpService
{
    public class UrlBuilder
    {
        public string Result { get; }

        public UrlBuilder(UrlController controller, long? id = null)
        {
            Result = string.Intern($"{SetController(controller)}");
            if (id.HasValue)
                Result += string.Intern($"/{id.Value}");
        }
        public UrlBuilder(UrlController controller, UrlOption option)
        {
            Result = string.Intern($"{SetController(controller)}/{SetOption(option)}/");
        }
        public UrlBuilder(UrlController controller, long id, UrlOption option)
        {
            Result = string.Intern($"{SetController(controller)}/{id}/{SetOption(option)}/");
        }
        public UrlBuilder(UrlController controller, UrlPath path, long id)
        {
            Result = string.Intern($"{SetController(controller)}/{SetPath(path)}/{id}");
        }
        public UrlBuilder(UrlController controller, UrlPath path, int value)
        {
            Result = string.Intern($"{SetController(controller)}/{SetPath(path)}/{value}");
        }
        public UrlBuilder(UrlController controller, UrlPath path, long id, UrlOption option)
        {
            Result = string.Intern($"{SetController(controller)}/{SetPath(path)}/{id}/{SetOption(option)}/");
        }
        public UrlBuilder(UrlController controller, UrlPath path, long id, UrlPath path2, long id2)
        {
            Result = string.Intern($"{SetController(controller)}/{SetPath(path)}/{id}/{SetPath(path2)}/{id2}");
        }
        public UrlBuilder(UrlController controller, UrlPath path, long id, UrlPath path2, long id2, UrlOption option)
        {
            Result = string.Intern($"{SetController(controller)}/{SetPath(path)}/{id}/{SetPath(path2)}/{id2}/{SetOption(option)}/");
        }

        public UrlBuilder(UrlCatalog catalog)
        {
            Result = string.Intern($"catalog/{SetCatalog(catalog)}/");
        }
        public UrlBuilder(UrlService service)
        {
            Result = string.Intern($"services/{SetService(service)}/");
        }

        static string SetController(UrlController controller) => controller switch
        {
            UrlController.Accounts => ControllerName.Accounts,
            UrlController.AccountTransactions => ControllerName.AccountTransactions,
            UrlController.BuyRecommendations => ControllerName.BuyRecommendations,
            UrlController.Coefficients => ControllerName.Coefficients,
            UrlController.Comissions => ControllerName.Comissions,
            UrlController.ComissionTypes => ControllerName.ComissionTypes,
            UrlController.Companies => ControllerName.Companies,
            UrlController.Dividends => ControllerName.Dividends,
            UrlController.ExchangeRates => ControllerName.ExchangeRates,
            UrlController.Isins => ControllerName.Isins,
            UrlController.Prices => ControllerName.Prices,
            UrlController.Ratings => ControllerName.Ratings,
            UrlController.Reports => ControllerName.Reports,
            UrlController.ReportSources => ControllerName.ReportSources,
            UrlController.SellRecommendations => ControllerName.SellRecommendations,
            UrlController.Services => ControllerName.Services,
            UrlController.StockTransactions => ControllerName.StockTransactions,
            UrlController.Tickers => ControllerName.Tickers,
            UrlController.Sectors => ControllerName.Sectors,
            UrlController.Industries => ControllerName.Industries,

            _ => throw new NotImplementedException()
        };
        static string SetPath(UrlPath path) => path switch
        {
            UrlPath.ByCompanyId => PathName.ByCompanyId,
            UrlPath.ByAccountId => PathName.ByAccountId,
            UrlPath.ByPagination => PathName.ByPagination,
            _ => ""
        };
        static string SetOption(UrlOption option) => option switch
        {
            UrlOption.New => OptionName.New,
            UrlOption.Last => OptionName.Last,
            UrlOption.Summary => OptionName.Summary,
            UrlOption.Additional => OptionName.Additional,
            _ => ""
        };
        static string SetCatalog(UrlCatalog catalog) => catalog switch
        {
            UrlCatalog.ComissionTypes => CatalogName.ComissionTypes,
            UrlCatalog.CurrencyTypes => CatalogName.CurrencyTypes,
            UrlCatalog.ExchangeTypes => CatalogName.ExchangeTypes,
            UrlCatalog.LotTypes => CatalogName.LotTypes,
            UrlCatalog.StatusTypes => CatalogName.StatusTypes,
            _ => ""
        };
        static string SetService(UrlService service) => service switch
        {
            UrlService.ResetCalculator => ServiceName.ResetCalculator,
            UrlService.ResetSummary => ServiceName.ResetSummary,
            UrlService.ParseBrokerReports => ServiceName.ParseBrokerReports,
            UrlService.ParseReports => ServiceName.ParseReports,
            UrlService.ParsePrices => ServiceName.ParsePrices,
            UrlService.Rate => ServiceName.Rate,
            _ => ""
        };
    }
    static class ControllerName
    {
        public const string Accounts = "accounts";
        public const string AccountTransactions = "accounttransactions";
        public const string BuyRecommendations = "buyrecommendations";
        public const string Coefficients = "coefficients";
        public const string Comissions = "comissions";
        public const string ComissionTypes = "comissiontypes";
        public const string Companies = "companies";
        public const string Dividends = "dividends";
        public const string ExchangeRates = "exchangerates";
        public const string Isins = "isins";
        public const string Prices = "prices";
        public const string Ratings = "ratings";
        public const string Reports = "reports";
        public const string ReportSources = "reportsources";
        public const string SellRecommendations = "sellrecommendations";
        public const string Services = "services";
        public const string StockTransactions = "stocktransactions";
        public const string Tickers = "tickers";
        public const string Sectors = "sectors";
        public const string Industries = "industries";
    }
    static class PathName
    {
        public const string ByPagination = "bypagination";
        public const string ByCompanyId = "bycompanyid";
        public const string ByAccountId = "byaccountid";
    }
    static class OptionName
    {
        public const string New = "new";
        public const string Last = "last";
        public const string Summary = "summary";
        public const string Additional = "additional";
    }
    static class CatalogName
    {
        public const string CurrencyTypes = "currencytypes";
        public const string ExchangeTypes = "exchangetypes";
        public const string ComissionTypes = "comissiontypes";
        public const string LotTypes = "lottypes";
        public const string StatusTypes = "statustypes";
    }
    static class ServiceName
    {
        public const string ResetCalculator = "resetcalculatordata";
        public const string ResetSummary = "resetsummarydata";
        public const string ParseBrokerReports = "parsebrokerreports";
        public const string ParseReports = "parsereports";
        public const string ParsePrices = "parseprices";
        public const string Rate = "rate";
    }
}
