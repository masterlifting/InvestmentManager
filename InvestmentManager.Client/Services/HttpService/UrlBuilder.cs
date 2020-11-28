using System;
using static InvestmentManager.Client.Configurations.EnumConfig;

namespace InvestmentManager.Client.Services.HttpService
{
    public class UrlBuilder
    {
        public string Result { get; }

        public UrlBuilder(UrlController controller, long id = default)
        {
            Result = string.Intern($"{SetController(controller)}");
            if (id != default)
                Result += string.Intern($"/{id}");
        }
        public UrlBuilder(UrlController controller, long id, UrlOption option)
        {
            Result = string.Intern($"{SetController(controller)}/{id}/{SetOption(option)}/");
        }
        public UrlBuilder(UrlController controller, UrlOption option, int? value = default)
        {
            Result = string.Intern($"{SetController(controller)}/{SetOption(option)}/");
            if (value.HasValue)
                Result += string.Intern($"{value}");
        }
        public UrlBuilder(UrlController controller, UrlPath path, long id, UrlOption option = UrlOption.None)
        {
            Result = string.Intern($"{SetController(controller)}/{SetPath(path)}/{id}");
            if (option != UrlOption.None)
                Result += $"/{SetOption(option)}/";
        }
        public UrlBuilder(UrlController controller, UrlPath path1, string values, UrlPath path2, long id, UrlOption option = UrlOption.None)
        {
            Result = string.Intern($"{SetController(controller)}/{SetPath(path1)}/{values}/{SetPath(path2)}/{id}");
            if (option != UrlOption.None)
                Result += $"/{SetOption(option)}/";
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
            UrlController.Comissions => ControllerName.Comissions,
            UrlController.Companies => ControllerName.Companies,
            UrlController.Dividends => ControllerName.Dividends,
            UrlController.ExchangeRates => ControllerName.ExchangeRates,
            UrlController.Isins => ControllerName.Isins,
            UrlController.Prices => ControllerName.Prices,
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
            UrlPath.ByAccountIds => PathName.ByAccountIds,
            _ => ""
        };
        static string SetOption(UrlOption option) => option switch
        {
            UrlOption.Pagination => OptionName.Pagination,
            UrlOption.OrderBy => OptionName.OrderBy,
            UrlOption.OrderDesc => OptionName.OrderDesc,
            UrlOption.New => OptionName.New,
            UrlOption.Last => OptionName.Last,
            UrlOption.Summary => OptionName.Summary,
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
            UrlService.RecalculateAll => ServiceName.RecalculateAll,
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
        public const string Comissions = "comissions";
        public const string Companies = "companies";
        public const string Dividends = "dividends";
        public const string ExchangeRates = "exchangerates";
        public const string Isins = "isins";
        public const string Prices = "prices";
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
        public const string Model = "model";
        public const string ByCompanyId = "bycompanyid";
        public const string ByAccountIds = "byaccountids";
    }
    static class OptionName
    {
        public const string Pagination = "pagination";
        public const string OrderBy = "orderby";
        public const string OrderDesc = "orderdesc";
        public const string New = "new";
        public const string Last = "last";
        public const string Summary = "summary";
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
        public const string RecalculateAll = "recalculateall";
        public const string ParseBrokerReports = "parsebrokerreports";
        public const string ParseReports = "parsereports";
        public const string ParsePrices = "parseprices";
        public const string Rate = "rate";
    }
}
