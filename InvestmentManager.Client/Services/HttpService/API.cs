using InvestmentManager.Models;
using InvestmentManager.Models.Additional;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.Security;
using InvestmentManager.Models.Services;
using InvestmentManager.Models.SummaryModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.HttpService
{
    public class API
    {
        private readonly CustomHttpClient http;
        public API(CustomHttpClient http)
        {
            Account = new(() => new(http));
            AccountTransaction = new(() => new(http));
            BuyRecommendation = new(() => new(http));
            Catalog = new(() => new(http));
            Coefficient = new(() => new(http));
            Comission = new(() => new(http));
            ComissionType = new(() => new(http));
            Company = new(() => new(http));
            Dividend = new(() => new(http));
            ExchangeRate = new(() => new(http));
            Industry = new(() => new(http));
            Isin = new(() => new(http));
            Price = new(() => new(http));
            Rating = new(() => new(http));
            Report = new(() => new(http));
            ReportSource = new(() => new(http));
            Sector = new(() => new(http));
            Security = new(() => new(http));
            SellRecommendation = new(() => new(http));
            Service = new(() => new(http));
            StockTransaction = new(() => new(http));
            Ticker = new(() => new(http));
            this.http = http;
        }

        public Lazy<AccountAPI> Account { get; }
        public Lazy<AccountTransactionAPI> AccountTransaction { get; }
        public Lazy<BuyRecommendationAPI> BuyRecommendation { get; }
        public Lazy<CatalogAPI> Catalog { get; }
        public Lazy<CoefficientAPI> Coefficient { get; }
        public Lazy<ComissionAPI> Comission { get; }
        public Lazy<ComissionTypeAPI> ComissionType { get; }
        public Lazy<CompanyAPI> Company { get; }
        public Lazy<DividendAPI> Dividend { get; }
        public Lazy<ExchangeRateAPI> ExchangeRate { get; }
        public Lazy<IndustryAPI> Industry { get; }
        public Lazy<IsinAPI> Isin { get; }
        public Lazy<PriceAPI> Price { get; }
        public Lazy<RatingAPI> Rating { get; }
        public Lazy<ReportAPI> Report { get; }
        public Lazy<ReportSourceAPI> ReportSource { get; }
        public Lazy<SectorAPI> Sector { get; }
        public Lazy<SecurityAPI> Security { get; }
        public Lazy<SellRecommendationAPI> SellRecommendation { get; }
        public Lazy<ServiceAPI> Service { get; }
        public Lazy<StockTransactionAPI> StockTransaction { get; }
        public Lazy<TickerAPI> Ticker { get; }

        public async Task<PaginationViewModel<ShortView>> GetByPaginationAsync(string controller, int value) =>
          await http.GetAsync<PaginationViewModel<ShortView>>(APIUriBuilder.UriPaginationByPage(controller, value));
    }
    static class APIUriBuilder
    {
        internal static string UriByAccount(string controller, long id) => controller + "/byaccountid/" + id;
        internal static string UriByCompany(string controller, long id) => controller + "/bycompanyid/" + id;
        internal static string UriByAccountByCompany(string controller, long accountId, long companyId) => controller + "/byaccountid/" + accountId + "/bycompanyid/" + companyId;

        internal static string UriSummaryById(string controller, long id) => controller + "/" + id + "/summary/";
        internal static string UriSummaryByAccount(string controller, long id) => controller + "/byaccountid/" + id + "/summary/";
        internal static string UriSummaryByCompany(string controller, long id) => controller + "/bycompanyid/" + id + "/summary/";
        internal static string UriSummaryByAccountByCompany(string controller, long accountId, long companyId) => controller + "/byaccountid/" + accountId + "/bycompanyid/" + companyId + "/summary/";

        internal static string UriAdditionalById(string controller, long id) => controller + "/" + id + "/additional/";
        internal static string UriPaginationByPage(string controller, int page) => controller + "/bypagination/" + page;
    }

    public class AccountAPI
    {
        private const string _controller = "accounts";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public AccountAPI(CustomHttpClient http) => this.http = http;

        public static string GetUriAddittionalById(long id) => APIUriBuilder.UriAdditionalById(_controller, id);
        public static string GetUriSummaryById(long id) => APIUriBuilder.UriSummaryById(_controller, id);

        public async Task<List<AccountModel>> GetAccountsAsync() => await http.GetAsync<List<AccountModel>>(_controller);
        public async Task<AccountModel> GetAccountAsync(long id) => await http.GetAsync<AccountModel>($"{_controller}/{id}");
        public async Task<decimal> GetAccountSumAsync(long id) => await http.GetAsync<decimal>(APIUriBuilder.UriSummaryById(_controller, id));
        public async Task<AccountAdditionalModel> GetAdditionalAsync(long id) => await http.GetAsync<AccountAdditionalModel>(APIUriBuilder.UriAdditionalById(_controller, id));
        public async Task<BaseActionResult> AddNewAsync(AccountModel model) => await http.PostAsync<BaseActionResult, AccountModel>(_controller, model);
    }
    public class AccountTransactionAPI
    {
        private const string _controller = "accounttransactions";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public AccountTransactionAPI(CustomHttpClient http) => this.http = http;

        public static string GetUriByAccountId(long id) => APIUriBuilder.UriByAccount(_controller, id);

        public async Task<List<AccountTransactionModel>> GetTransactionsByAccountIdAsync(long id) =>
            await http.GetAsync<List<AccountTransactionModel>>(APIUriBuilder.UriByAccount(_controller, id));
        public async Task<SummaryAccountTransaction> GetSummaryByAccountIdAsync(long id) =>
            await http.GetAsync<SummaryAccountTransaction>(APIUriBuilder.UriSummaryByAccount(_controller, id));
        public async Task<BaseActionResult> AddNewAsync(AccountTransactionModel model) =>
            await http.PostAsync<BaseActionResult, AccountTransactionModel>(_controller, model);

    }
    public class BuyRecommendationAPI
    {
        private const string _controller = "buyrecommendations";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public BuyRecommendationAPI(CustomHttpClient http) => this.http = http;

        public static string GetUriSummaryByCompanyId(long id) => APIUriBuilder.UriSummaryByCompany(_controller, id);

        public async Task<PaginationViewModel<ShortView>> GetRecommendationsByPaginationAsync(int value) =>
            await http.GetAsync<PaginationViewModel<ShortView>>(APIUriBuilder.UriPaginationByPage(_controller, value));
        public async Task<BuyRecommendationModel> GetRecommendationByCompanyIdAsync(long id) =>
            await http.GetAsync<BuyRecommendationModel>(APIUriBuilder.UriByCompany(_controller, id));
        public async Task<SummaryBuyRecommendation> GetSummaryByCompanyIdAsync(long id) =>
            await http.GetAsync<SummaryBuyRecommendation>(APIUriBuilder.UriSummaryByCompany(_controller, id));
    }
    public class CatalogAPI
    {
        private const string _controller = "catalog";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public CatalogAPI(CustomHttpClient http) => this.http = http;

        public async Task<List<ShortView>> GetCurrencyTypesAsync() => await http.GetAsync<List<ShortView>>(_controller + "/currencytypes/");
        public async Task<List<ShortView>> GetExchangeTypesAsync() => await http.GetAsync<List<ShortView>>(_controller + "/exchangetypes/");
        public async Task<List<ShortView>> GetComissionTypesAsync() => await http.GetAsync<List<ShortView>>(_controller + "/comissiontypes/");
        public async Task<List<ShortView>> GetLotTypesAsync() => await http.GetAsync<List<ShortView>>(_controller + "/lottypes/");
        public async Task<List<ShortView>> GetStatusTypesAsync() => await http.GetAsync<List<ShortView>>(_controller + "/statustypes/");
    }
    public class CoefficientAPI
    {
        private const string _controller = "coefficients";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public CoefficientAPI(CustomHttpClient http) => this.http = http;

        public static string GetUriByCompanyId(long id) => APIUriBuilder.UriByCompany(_controller, id);
        public static string GetUriSummaryByCompanyId(long id) => APIUriBuilder.UriSummaryByCompany(_controller, id);

        public async Task<List<CoefficientModel>> GetCoefficientsByCompanyIdAsync(long id) => await http.GetAsync<List<CoefficientModel>>(APIUriBuilder.UriByCompany(_controller, id));
        public async Task<SummaryCoefficient> GetSummaryByCompanyIdAsync(long id) => await http.GetAsync<SummaryCoefficient>(APIUriBuilder.UriSummaryByCompany(_controller, id));

    }
    public class ComissionAPI
    {
        private const string _controller = "comissions";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public ComissionAPI(CustomHttpClient http) => this.http = http;

        public static string GetUriByAccountId(long id) => APIUriBuilder.UriByAccount(_controller, id);
        public static string GetUriSummaryByAccountId(long id) => APIUriBuilder.UriSummaryByCompany(_controller, id);

        public async Task<List<ComissionModel>> GetComissionsByCompanyIdAsync(long id) => await http.GetAsync<List<ComissionModel>>(APIUriBuilder.UriByCompany(_controller, id));
        public async Task<SummaryComission> GetSummaryByCompanyId(long id) => await http.GetAsync<SummaryComission>(APIUriBuilder.UriSummaryByCompany(_controller, id));
        public async Task<BaseActionResult> AddNewAsync(ComissionModel model) => await http.PostAsync<BaseActionResult, ComissionModel>(_controller, model);

    }
    public class ComissionTypeAPI
    {
        private const string _controller = "comissiontypes";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public ComissionTypeAPI(CustomHttpClient http) => this.http = http;

        public async Task<BaseActionResult> AddNewAsync(ComissionTypeModel model) => await http.PostAsync<BaseActionResult, ComissionTypeModel>(_controller, model);
    }
    public class CompanyAPI
    {
        private const string _controller = "companies";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public CompanyAPI(CustomHttpClient http) => this.http = http;

        public static string GetUriAddittionalByCompanyId(long id) => APIUriBuilder.UriAdditionalById(_controller, id);

        public async Task<List<ShortView>> GetShortCompaniesAsync() => await http.GetAsync<List<ShortView>>(_controller);
        public async Task<CompanyModel> GetCompanyAsync(long id) => await http.GetAsync<CompanyModel>($"{_controller}/{id}");
        public async Task<CompanyAdditionalModel> GetAdditionalAsync(long id) => await http.GetAsync<CompanyAdditionalModel>(APIUriBuilder.UriAdditionalById(_controller, id));
        public async Task<PaginationViewModel<ShortView>> GetCompaniesByPaginationAsync(int value) =>
            await http.GetAsync<PaginationViewModel<ShortView>>(APIUriBuilder.UriPaginationByPage(_controller, value));
        public async Task<BaseActionResult> AddNewAsync(CompanyModel model) => await http.PostAsync<BaseActionResult, CompanyModel>(_controller, model);
        public async Task<BaseActionResult> EditAsync(long id, CompanyModel model) => await http.PutAsync<BaseActionResult, CompanyModel>($"{_controller}/{id}", model);
    }
    public class DividendAPI
    {
        private const string _controller = "dividends";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public DividendAPI(CustomHttpClient http) => this.http = http;

        public static string GetUriByAccountIdByCompanyId(long accountId, long companyId) => APIUriBuilder.UriByAccountByCompany(_controller, accountId, companyId);
        public static string GetUriSummaryByAccountIdByCompanyId(long accountId, long companyId) => APIUriBuilder.UriSummaryByAccountByCompany(_controller, accountId, companyId);

        public async Task<PaginationViewModel<ShortView>> GetDividendsByPaginationAsync(int value) =>
            await http.GetAsync<PaginationViewModel<ShortView>>(APIUriBuilder.UriPaginationByPage(_controller, value));
        public async Task<List<DividendModel>> GetDividendsByAccountIdByCompanyIdAsync(long accountId, long companyId) =>
            await http.GetAsync<List<DividendModel>>(APIUriBuilder.UriByAccountByCompany(_controller, accountId, companyId));
        public async Task<SummaryDividend> GetSummaryByAccountIdByCompanyIdAsync(long accountId, long companyId) =>
            await http.GetAsync<SummaryDividend>(APIUriBuilder.UriSummaryByAccountByCompany(_controller, accountId, companyId));
        public async Task<BaseActionResult> AddNewAsync(DividendModel model) => await http.PostAsync<BaseActionResult, DividendModel>(_controller, model);
    }
    public class ExchangeRateAPI
    {
        private const string _controller = "exchangerates";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;

        public static string GetUriByAccountId(long id) => APIUriBuilder.UriByAccount(_controller, id);
        public ExchangeRateAPI(CustomHttpClient http) => this.http = http;

        public static string GetUriSummaryByAccountId(long id) => APIUriBuilder.UriSummaryByAccount(_controller, id);

        public async Task<List<ExchangeRateModel>> GetExchangeRatesByAccountIdAsync(long id) => await http.GetAsync<List<ExchangeRateModel>>(APIUriBuilder.UriByAccount(_controller, id));
        public async Task<SummaryExchangeRate> GetSummaryByAccountIdAsync(long id) => await http.GetAsync<SummaryExchangeRate>(APIUriBuilder.UriSummaryByAccount(_controller, id));
        public async Task<BaseActionResult> AddNewAsync(ExchangeRateModel model) => await http.PostAsync<BaseActionResult, ExchangeRateModel>(_controller, model);
    }
    public class IndustryAPI
    {
        private const string _controller = "industries";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public IndustryAPI(CustomHttpClient http) => this.http = http;
        public async Task<PaginationViewModel<ShortView>> GetIndustriesByPaginationAsync(int value) =>
            await http.GetAsync<PaginationViewModel<ShortView>>(APIUriBuilder.UriPaginationByPage(_controller, value));
        public async Task<List<ShortView>> GetIndustriesAsync() => await http.GetAsync<List<ShortView>>(_controller);
        public async Task<ShortView> GetIndustryAsync(long id) => await http.GetAsync<ShortView>($"{_controller}/{id}");
    }
    public class IsinAPI
    {
        private const string _controller = "isins";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public IsinAPI(CustomHttpClient http) => this.http = http;
        public async Task<List<IsinModel>> GetIsinsAsync() => await http.GetAsync<List<IsinModel>>(_controller);
        public async Task<List<IsinModel>> GetIsinsByCompanyIdAsync(long id) => await http.GetAsync<List<IsinModel>>(APIUriBuilder.UriByCompany(_controller, id));
        public async Task<BaseActionResult> AddNewAsync(IsinModel model) => await http.PostAsync<BaseActionResult, IsinModel>(_controller, model);
        public async Task<BaseActionResult> EditAsync(long id, IsinModel model) => await http.PutAsync<BaseActionResult, IsinModel>($"{_controller}/{id}", model);
        public async Task<BaseActionResult> DeleteAsync(long id) => await http.DeleteAsync<BaseActionResult>($"{_controller}/{id}");
    }
    public class PriceAPI
    {
        private const string _controller = "prices";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public PriceAPI(CustomHttpClient http) => this.http = http;

        public static string GetUriByCompanyId(long id) => APIUriBuilder.UriByCompany(_controller, id);
        public static string GetUriSummaryByCompanyId(long id) => APIUriBuilder.UriSummaryByCompany(_controller, id);

        public async Task<List<PriceModel>> GetPricesByCompanyIdAsync(long id) => await http.GetAsync<List<PriceModel>>(APIUriBuilder.UriByCompany(_controller, id));
        public async Task<SummaryPrice> GetSummaryByCompanyIdAsync(long id) => await http.GetAsync<SummaryPrice>(APIUriBuilder.UriSummaryByCompany(_controller, id));
    }
    public class RatingAPI
    {
        private const string _controller = "ratings";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public RatingAPI(CustomHttpClient http) => this.http = http;

        public static string GetUriByCompanyId(long id) => APIUriBuilder.UriByCompany(_controller, id);
        public static string GetUriSummaryByCompanyId(long id) => APIUriBuilder.UriSummaryByCompany(_controller, id);

        public async Task<PaginationViewModel<ShortView>> GetRatingsPaginationAsync(int value) => await http.GetAsync<PaginationViewModel<ShortView>>(APIUriBuilder.UriPaginationByPage(_controller, value));
        public async Task<List<RatingModel>> GetByCompanyIdAsync(long id) => await http.GetAsync<List<RatingModel>>(APIUriBuilder.UriByCompany(_controller, id));
        public async Task<SummaryRating> GetSummaryByCompanyIdAsync(long id) => await http.GetAsync<SummaryRating>(APIUriBuilder.UriSummaryByCompany(_controller, id));
    }
    public class ReportAPI
    {
        private const string _controller = "reports";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public ReportAPI(CustomHttpClient http) => this.http = http;

        public static string GetUriByCompanyId(long id) => APIUriBuilder.UriByCompany(_controller, id);
        public static string GetUriSummaryByCompanyId(long id) => APIUriBuilder.UriSummaryByCompany(_controller, id);

        public async Task<List<ReportModel>> GetReportsByCompanyIdAsync(long id) => await http.GetAsync<List<ReportModel>>(APIUriBuilder.UriByCompany(_controller, id));
        public async Task<SummaryReport> GetSummaryByCompanyIdAsync(long id) => await http.GetAsync<SummaryReport>(APIUriBuilder.UriSummaryByCompany(_controller, id));
        public async Task<List<ReportModel>> GetUncheckedReportsAsync() => await http.GetAsync<List<ReportModel>>(_controller + "/new/");
        public async Task<BaseActionResult> EditAsync(long id, ReportModel model) => await http.PutAsync<BaseActionResult, ReportModel>($"{_controller}/{id}", model);
        public async Task<BaseActionResult> DeleteAsync(long id) => await http.DeleteAsync<BaseActionResult>($"{_controller}/{id}");
    }
    public class ReportSourceAPI
    {
        private const string _controller = "reportsources";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public ReportSourceAPI(CustomHttpClient http) => this.http = http;
        public async Task<ReportSourceModel> GetReportSourceByCompanyIdAsync(long id) => await http.GetAsync<ReportSourceModel>(APIUriBuilder.UriByCompany(_controller, id));
        public async Task<BaseActionResult> AddNewAsync(ReportSourceModel model) => await http.PostAsync<BaseActionResult, ReportSourceModel>(_controller, model);
        public async Task<BaseActionResult> EditAsync(long id, ReportSourceModel model) => await http.PutAsync<BaseActionResult, ReportSourceModel>($"{_controller}/{id}", model);

    }
    public class SectorAPI
    {
        private const string _controller = "sectors";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public SectorAPI(CustomHttpClient http) => this.http = http;
        public async Task<PaginationViewModel<ShortView>> GetSectorsByPaginationAsync(int value) =>
           await http.GetAsync<PaginationViewModel<ShortView>>(APIUriBuilder.UriPaginationByPage(_controller, value));
        public async Task<List<ShortView>> GetSectorsAsync() => await http.GetAsync<List<ShortView>>(_controller);
        public async Task<ShortView> GetSectorAsync(long id) => await http.GetAsync<ShortView>($"{_controller}/{id}");
    }
    public class SecurityAPI
    {
        private const string _controller = "security";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public SecurityAPI(CustomHttpClient http) => this.http = http;
        public async Task<AuthResult> LoginAsync(LoginModel model) => await http.PostAsync<AuthResult, LoginModel>($"{_controller}/login/", model);
        public async Task<AuthResult> RegisterAsync(RegisterModel model) => await http.PostAsync<AuthResult, RegisterModel>($"{_controller}/register/", model);
    }
    public class SellRecommendationAPI
    {
        private const string _controller = "sellrecommendations";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public SellRecommendationAPI(CustomHttpClient http) => this.http = http;

        public static string GetUriSummaryByCompanyId(long id) => APIUriBuilder.UriSummaryByCompany(_controller, id);

        public async Task<PaginationViewModel<ShortView>> GetRecommendationsByPaginationAsync(int value) =>
            await http.GetAsync<PaginationViewModel<ShortView>>(APIUriBuilder.UriPaginationByPage(_controller, value));
        public async Task<SellRecommendationModel> GetRecommendationByCompanyIdAsync(long id) =>
            await http.GetAsync<SellRecommendationModel>(APIUriBuilder.UriByCompany(_controller, id));
        public async Task<SummarySellRecommendation> GetSummaryByCompanyIdAsync(long id) =>
            await http.GetAsync<SummarySellRecommendation>(APIUriBuilder.UriSummaryByCompany(_controller, id));
    }
    public class ServiceAPI
    {
        private const string _controller = "services";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public ServiceAPI(CustomHttpClient http) => this.http = http;
        public async Task<CBRF> GetRateAsync() => await http.GetAsync<CBRF>(_controller + "/rate/");
        public async Task<BrokerReportModel> ParseBcsReportsAsync(HttpContent content) => await http.PostContentAsync<BrokerReportModel>(_controller + "/parsebrokerreports/", content);
        public async Task<BaseActionResult> ParsePricesAsync() => await http.GetAsync<BaseActionResult>(_controller + "/parseprices/");
        public async Task ParseReportsAsync() => await http.GetAsync(_controller + "/parsereports/");
        public async Task<BaseActionResult> ResetCalculatorDataAsync() => await http.GetAsync<BaseActionResult>(_controller + "/resetcalculatordata/");
        public async Task<BaseActionResult> ResetSummaryDataAsync() => await http.GetAsync<BaseActionResult>(_controller + "/resetsummarydata/");
    }
    public class StockTransactionAPI
    {
        private const string _controller = "stocktransactions";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public StockTransactionAPI(CustomHttpClient http) => this.http = http;

        public static string GetUriByAccountIdByCompanyId(long accountId, long companyId) => APIUriBuilder.UriByAccountByCompany(_controller, accountId, companyId);
        public static string GetUriSummaryByAccountIdByCompanyId(long accountId, long companyId) => APIUriBuilder.UriSummaryByAccountByCompany(_controller, accountId, companyId);

        public async Task<PaginationViewModel<ShortView>> GetTransactionsByPaginationAsync(int value) =>
          await http.GetAsync<PaginationViewModel<ShortView>>(APIUriBuilder.UriPaginationByPage(_controller, value));
        public async Task<List<StockTransactionModel>> GetTransactionsByAccountIdByCompanyIdAsync(long accountId, long companyId) =>
            await http.GetAsync<List<StockTransactionModel>>(APIUriBuilder.UriByAccountByCompany(_controller, accountId, companyId));
        public async Task<SummaryStockTransaction> GetSummaryByAccountIdByCompanyIdAsync(long accountId, long companyId) =>
            await http.GetAsync<SummaryStockTransaction>(APIUriBuilder.UriSummaryByAccountByCompany(_controller, accountId, companyId));
        public async Task<BaseActionResult> AddNewAsync(StockTransactionModel model) => await http.PostAsync<BaseActionResult, StockTransactionModel>(_controller, model);
    }
    public class TickerAPI
    {
        private const string _controller = "tickers";
        public static readonly string controller = _controller;

        private readonly CustomHttpClient http;
        public TickerAPI(CustomHttpClient http) => this.http = http;
        public async Task<List<TickerModel>> GeTickersAsync() => await http.GetAsync<List<TickerModel>>(_controller);
        public async Task<List<TickerModel>> GetTickersByCompanyIdAsync(long id) => await http.GetAsync<List<TickerModel>>(APIUriBuilder.UriByCompany(_controller, id));
        public async Task<BaseActionResult> AddNewAsync(TickerModel model) => await http.PostAsync<BaseActionResult, TickerModel>(_controller, model);
        public async Task<BaseActionResult> EditAsync(long id, TickerModel model) => await http.PutAsync<BaseActionResult, TickerModel>($"{_controller}/{id}", model);
        public async Task<BaseActionResult> DeleteAsync(long id) => await http.DeleteAsync<BaseActionResult>($"{_controller}/{id}");
    }
}
