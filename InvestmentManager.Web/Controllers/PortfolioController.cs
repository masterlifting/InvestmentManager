using InvestmentManager.BrokerService;
using InvestmentManager.BrokerService.Models;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Market;
using InvestmentManager.Repository;
using InvestmentManager.Service.Interfaces;
using InvestmentManager.Web.Models.PortfolioModels;
using InvestmentManager.Web.ViewAgregator.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InvestmentManager.Web.Controllers
{
    [Authorize]
    public class PortfolioController : Controller
    {
        private static readonly Dictionary<string, List<BrokerReportModel>> BrokerReportModels = new Dictionary<string, List<BrokerReportModel>>();
        private static readonly Dictionary<string, List<PortfolioReportModel>> ResultPortfolioReports = new Dictionary<string, List<PortfolioReportModel>>();

        private readonly IWebHostEnvironment environment;
        private readonly IIOService loaderService;
        private readonly IInvestBrokerService brokerService;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IPortfolioAgregator agregator;
        const string controllerName = "~/portfolio";
        public PortfolioController(
            IWebHostEnvironment environment
            , IIOService loaderService
            , IInvestBrokerService brokerService
            , UserManager<IdentityUser> userManager
            , IUnitOfWorkFactory unitOfWork
            , IPortfolioAgregator agregator)
        {
            this.environment = environment;
            this.loaderService = loaderService;
            this.brokerService = brokerService;
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.agregator = agregator;
        }

        [HttpPost]
        public async Task<IActionResult> LoadReportFiles(IFormFileCollection files)
        {
            string path = "";

            if (environment?.WebRootPath != null)
                path = @$"{environment.WebRootPath}\UserFiles\{userManager.GetUserId(User)}\Reports\";

            if (string.IsNullOrWhiteSpace(path))
                throw new NullReferenceException($"Не удалось определить путь для отчета: {path}");
            if (files is null || !files.Any())
                throw new NullReferenceException($"Не удалось получить файлы для загрузки: {files}");

            // Если такого пути еще нет, то создаю
            var directory = new DirectoryInfo(path);
            if (!directory.Exists)
                directory.Create();

            // Загружу на сервер файлы
            foreach (var file in files)
            {
                Match match = Regex.Match(Path.GetFileNameWithoutExtension(file.FileName), @"B_k(.+)_ALL(.+)");
                if (match.Success && Path.GetExtension(file.FileName).Equals(".xls"))
                {
                    string filePath = $"{path}{file.FileName}";
                    using FileStream stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream).ConfigureAwait(false);
                }
            }

            return RedirectToAction(nameof(ParseLoadedReports), "portfolio", new { path });
        }
        public IActionResult ParseLoadedReports(string path)
        {
            string userId = userManager.GetUserId(User);

            var brokerReports = new List<BrokerReportModel>();
            var firstStepData = new Dictionary<FilterReportModel, DataSet>();
            var reportsPath = Directory.GetFiles(path);

            foreach (var i in reportsPath)
            {
                using DataSet dataSet = loaderService.LoadDataSetFromExcel(i);
                firstStepData.Add(brokerService.BcsParser.ParsePeriodReport(dataSet), dataSet);
            }

            var uniqueReports = brokerService.ReportFilter.GetUniqueLoadedReports(firstStepData.Keys);

            foreach (var i in uniqueReports)
            {
                brokerReports.Add(brokerService.BcsParser.ParseBcsReport(firstStepData[i]));
            }

            if (BrokerReportModels.ContainsKey(userId))
                BrokerReportModels.Remove(userId);

            // Очищу разобранные файлы
            var directory = new DirectoryInfo(path);
            var reportFiles = directory.GetFiles();

            foreach (var file in reportFiles)
            {
                file.Refresh();
                file.Delete();
            }

            BrokerReportModels.Add(userId, brokerReports);

            return LocalRedirect($"{controllerName}/{nameof(ShowLoadedFiles)}");
        }
        public IActionResult ShowLoadedFiles() => BrokerReportModels.TryGetValue(userManager.GetUserId(User), out List<BrokerReportModel> resultReports)
                ? View(resultReports)
                : View(new List<BrokerReportModel>());
        public async Task<IActionResult> ShowErrorReports()
        {
            string userId = userManager.GetUserId(User);

            var errorPortfolioReportModels = new Dictionary<List<string>, BrokerReportModel>();
            var portfolioReportModels = new List<PortfolioReportModel>();
            var errorReportModels = new List<BrokerReportModel>();
            List<BrokerReportModel> currentReports = new List<BrokerReportModel>();

            if (BrokerReportModels.ContainsKey(userId))
                currentReports = BrokerReportModels[userId];
            if (ResultPortfolioReports.ContainsKey(userId))
                ResultPortfolioReports.Remove(userId);

            foreach (var i in currentReports)
            {
                var portfolioReportModel = new PortfolioReportModel();

                var errorReportModel = new BrokerReportModel
                {
                    AccountId = i.AccountId,
                    DateBeginReport = i.DateBeginReport,
                    DateEndReport = i.DateEndReport
                };

                var errors = new List<string>();

                var account = await unitOfWork.Account.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.AccountId)).ConfigureAwait(false);

                if (account is null)
                {
                    errors.Add($"Добавьте в свой профиль соглашение \'{i.AccountId}\'");
                    errorPortfolioReportModels.Add(errors, errorReportModel);
                    return View(errorPortfolioReportModels);
                }

                portfolioReportModel.AccountName = account?.Name;
                portfolioReportModel.DateBeginReport = i.DateBeginReport;
                portfolioReportModel.DateEndReport = i.DateEndReport;

                try
                {
                    portfolioReportModel.AccountTransactions = await brokerService.ReportMapper.MapToAccountTransactionsAsync(i.AccountTransactions, account.Id).ConfigureAwait(false);
                }
                catch (SystemException exeption)
                {
                    errorReportModel.AccountTransactions = i.AccountTransactions;
                    errors.Add(exeption.Message);
                }
                try
                {
                    portfolioReportModel.StockTransactions = await brokerService.ReportMapper.MapToStockTransactionsAsync(i.StockTransactions, account.Id).ConfigureAwait(false);
                }
                catch (SystemException exeption)
                {
                    errorReportModel.StockTransactions = i.StockTransactions;
                    errors.Add(exeption.Message);
                }
                try
                {
                    portfolioReportModel.Dividends = await brokerService.ReportMapper.MapToDividendsAsync(i.Dividends, account.Id).ConfigureAwait(false);
                }
                catch (SystemException exeption)
                {
                    errorReportModel.Dividends = i.Dividends;
                    errors.Add(exeption.Message);
                }
                try
                {
                    portfolioReportModel.Comissions = await brokerService.ReportMapper.MapToComissionsAsync(i.Comissions, account.Id).ConfigureAwait(false);
                }
                catch (SystemException exeption)
                {
                    errorReportModel.Comissions = i.Comissions;
                    errors.Add(exeption.Message);
                }
                try
                {
                    portfolioReportModel.ExchangeRates = await brokerService.ReportMapper.MapToExchangeRatesAsync(i.ExchangeRates, account.Id).ConfigureAwait(false);
                }
                catch (SystemException exeption)
                {
                    errorReportModel.ExchangeRates = i.ExchangeRates;
                    errors.Add(exeption.Message);
                }

                portfolioReportModels.Add(portfolioReportModel);

                if (errors.Any())
                    errorPortfolioReportModels.Add(errors, errorReportModel);
            }

            ResultPortfolioReports.Add(userId, portfolioReportModels);

            return errorPortfolioReportModels.Any()
                ? View(errorPortfolioReportModels)
                : (IActionResult)LocalRedirect($"{controllerName}/{nameof(ShowResultReports)}");
        }
        public IActionResult ShowResultReports()
        {
            string userId = userManager.GetUserId(User);

            if (BrokerReportModels.ContainsKey(userId))
                BrokerReportModels.Remove(userId);

            var resultViewModel = new ResultBrokerReportViewModel();

            var checkedBrokerReports = ResultPortfolioReports[userId];

            if (checkedBrokerReports.Any())
                resultViewModel = agregator.GetResultReportView(checkedBrokerReports);

            return View(resultViewModel);
        }
        public async Task<IActionResult> SaveNewReports()
        {
            string userId = userManager.GetUserId(User);

            if (ResultPortfolioReports.ContainsKey(userId))
            {
                var resultReports = ResultPortfolioReports[userId];

                var resultAccountTransactions = new List<AccountTransaction>();
                var resultStockTransactions = new List<StockTransaction>();
                var resultDividends = new List<Dividend>();
                var resultComissions = new List<Comission>();
                var resultExchangeRates = new List<ExchangeRate>();

                foreach (var i in resultReports)
                {
                    resultAccountTransactions.AddRange(i.AccountTransactions);
                    resultStockTransactions.AddRange(i.StockTransactions);
                    resultDividends.AddRange(i.Dividends);
                    resultComissions.AddRange(i.Comissions);
                    resultExchangeRates.AddRange(i.ExchangeRates);
                }

                var filter = brokerService.ReportFilter;

                // variables to save
                if (resultAccountTransactions.Any())
                {
                    var accountTransactions = filter.GetNewTransactions(resultAccountTransactions);
                    if (accountTransactions.Any())
                        await unitOfWork.AccountTransaction.CreateEntitiesAsync(accountTransactions).ConfigureAwait(false);
                }
                if (resultStockTransactions.Any())
                {
                    var stockTransactions = filter.GetNewTransactions(resultStockTransactions);
                    if (stockTransactions.Any())
                        await unitOfWork.StockTransaction.CreateEntitiesAsync(stockTransactions).ConfigureAwait(false);
                }
                if (resultDividends.Any())
                {
                    var dividends = filter.GetNewTransactions(resultDividends);
                    if (dividends.Any())
                        await unitOfWork.Dividend.CreateEntitiesAsync(dividends).ConfigureAwait(false);
                }
                if (resultComissions.Any())
                {
                    var comissions = filter.GetNewTransactions(resultComissions);
                    if (comissions.Any())
                        await unitOfWork.Comission.CreateEntitiesAsync(comissions).ConfigureAwait(false);
                }
                if (resultExchangeRates.Any())
                {
                    var exchangeRates = filter.GetNewTransactions(resultExchangeRates);
                    if (exchangeRates.Any())
                        await unitOfWork.ExchangeRate.CreateEntitiesAsync(exchangeRates).ConfigureAwait(false);
                }

                await unitOfWork.CompleteAsync().ConfigureAwait(false);

                ResultPortfolioReports.Remove(userId);
            }

            return LocalRedirect($"~/Home/{nameof(HomeController.Index)}");
        }

        #region Report Error Handler
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<LocalRedirectResult> StockTransactionErrorHandler(StockTransactionErrorForm form)
        {
            if (form != null && ModelState.IsValid)
            {
                await unitOfWork.Ticker.CreateEntityAsync(new Ticker
                {
                    CompanyId = form.CompanyId,
                    ExchangeId = form.ExchangeId,
                    LotId = form.LotId,

                    Name = form.TickerName
                }).ConfigureAwait(false);

                await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            return LocalRedirect($"{controllerName}/{nameof(ShowErrorReports)}");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<LocalRedirectResult> AccountTransactionErrorHandler(AccountTransactionErrorForm form)
        {
            if (form != null && ModelState.IsValid)
            {
                await unitOfWork.Account.CreateEntityAsync(new Account
                {
                    Name = form.AccountName,
                    UserId = userManager.GetUserId(User)
                }).ConfigureAwait(false);

                await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            return LocalRedirect($"{controllerName}/{nameof(ShowErrorReports)}");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<LocalRedirectResult> DividendErrorHandler(DividendErrorForm form)
        {
            if (form != null && ModelState.IsValid)
            {
                await unitOfWork.Isin.CreateEntityAsync(new Isin
                {
                    CompanyId = form.CompanyId,
                    Name = form.Isin
                }).ConfigureAwait(false);

                await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            return LocalRedirect($"{controllerName}/{nameof(ShowErrorReports)}");
        }
        #endregion

        #region Dividends,Comissions,Operations
        public IActionResult Dividends()
        {
            var thisAccounts = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userManager.GetUserId(User)));
            var dividends = unitOfWork.Dividend.GetAll().Join(thisAccounts, x => x.AccountId, y => y.Id, (x, y) => x);

            var companies = unitOfWork.Company.GetAll();
            var isins = unitOfWork.Isin.GetAll();
            var currencies = unitOfWork.Currency.GetAll();

            var agregatedDividends = dividends.Join(isins, x => x.IsinId, y => y.Id, (x, y) => new { x.Amount, x.DateOperation, x.CurrencyId, y.CompanyId })
                                        .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new { x.Amount, x.DateOperation, x.CurrencyId, CompanyName = y.Name })
                                        .Join(currencies, x => x.CurrencyId, y => y.Id, (x, y) => new { x.Amount, x.DateOperation, x.CompanyName, CurrencyType = y.Name })
                                        .AsEnumerable();

            var resultDividends = agregatedDividends.GroupBy(x => x.CompanyName)
                                    .Select(x => new DividendModel
                                    {
                                        CompanyName = x.Key,
                                        DividendSum = Math.Round(x.Sum(y => y.Amount), 2),
                                        CurrencyType = x.First().CurrencyType,
                                        LastDate = x.OrderBy(y => y.DateOperation.Date).Last().DateOperation.Date,
                                        PayCount = x.Select(y => y.Amount).Count()
                                    });

            return View(resultDividends);
        }
        public IActionResult Comissions()
        {
            var model = new List<ComissionModel>();

            var thisAccounts = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userManager.GetUserId(User)));
            var comissions = unitOfWork.Comission.GetAll().Join(thisAccounts, x => x.AccountId, y => y.Id, (x, y) => x).AsEnumerable();

            var agregatedComissions = comissions.GroupBy(x => x.DateOperation.Year)
                                                .Select(x => new
                                                {
                                                    Year = x.Key,
                                                    Months = x.GroupBy(y => y.DateOperation.Month)
                                                });

            foreach (var i in agregatedComissions)
            {
                foreach (var j in i.Months)
                {
                    model.Add(new ComissionModel
                    {
                        Year = i.Year,
                        Month = j.Key,
                        Sum = j.Sum(x => x.Amount),
                    });
                }
            }
            return View(model);
        }
        public IActionResult StockOperations()
        {
            var thisAccounts = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userManager.GetUserId(User)));
            var transactions = unitOfWork.StockTransaction.GetAll().Join(thisAccounts, x => x.AccountId, y => y.Id, (x, y) => x);

            var companies = unitOfWork.Company.GetAll();
            var tickers = unitOfWork.Ticker.GetAll();
            var currencies = unitOfWork.Currency.GetAll();
            var lots = unitOfWork.Lot.GetAll();

            var agregateOperations = transactions
                                                   .Join(tickers, x => x.TickerId, y => y.Id, (x, y) => new
                                                   { x.TransactionStatusId, x.DateOperation, x.Quantity, x.Cost, x.CurrencyId, y.CompanyId, y.LotId })
                                                   .Join(lots, x => x.LotId, y => y.Id, (x, y) => new
                                                   { x.TransactionStatusId, x.DateOperation, x.Quantity, x.Cost, x.CurrencyId, x.CompanyId, LotValue = y.Value })
                                                   .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new
                                                   { x.CompanyId, x.TransactionStatusId, x.DateOperation, x.Quantity, x.Cost, x.CurrencyId, x.LotValue, CompanyName = y.Name })
                                                   .Join(currencies, x => x.CurrencyId, y => y.Id, (x, y) => new
                                                   { x.CompanyId, x.TransactionStatusId, x.DateOperation, x.Quantity, x.Cost, x.CurrencyId, x.CompanyName, x.LotValue, CurrencyType = y.Name })
                                                   .AsEnumerable();

            var resultOperations = agregateOperations.GroupBy(x => new { x.CompanyId, x.CompanyName }).Select(x => new StockOperationModel
            {
                CompanyId = x.Key.CompanyId,
                CompanyName = x.Key.CompanyName,
                FreeLot = (x.Where(y => y.TransactionStatusId == 3).Sum(x => x.Quantity / x.LotValue) - x.Where(y => y.TransactionStatusId == 4).Sum(x => x.Quantity / x.LotValue)),
                DateLastOperation = x.OrderBy(x => x.DateOperation.Date).Last().DateOperation,
                Profit = Math.Round(x.Where(y => y.TransactionStatusId == 4).Sum(y => y.Cost * y.Quantity) - x.Where(y => y.TransactionStatusId == 3).Sum(y => y.Cost * y.Quantity), 2),
                CurrencyType = x.First().CurrencyType
            });

            return View(resultOperations);
        }
        public IActionResult AccountOperations()
        {
            var thisAccounts = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userManager.GetUserId(User)));
            var transactions = unitOfWork.AccountTransaction.GetAll().Join(thisAccounts, x => x.AccountId, y => y.Id, (x, y) => x);

            var currencies = unitOfWork.Currency.GetAll();
            var statuses = unitOfWork.TransactionStatus.GetAll();

            var resultOperations = transactions
                                                    .Join(statuses, x => x.TransactionStatusId, y => y.Id, (x, y) => new
                                                    { x.AccountId, x.CurrencyId, x.Amount, x.DateOperation, TypeOperation = y.Name })
                                                    .Join(thisAccounts, x => x.AccountId, y => y.Id, (x, y) => new
                                                    { x.AccountId, x.CurrencyId, x.Amount, x.DateOperation, x.TypeOperation, AccountName = y.Name })
                                                    .Join(currencies, x => x.CurrencyId, y => y.Id, (x, y) => new
                                                    { x.AccountId, x.Amount, x.DateOperation, x.TypeOperation, x.AccountName, CurrencyType = y.Name })
                                                    .Select(x => new AccountOperationModel
                                                    {
                                                        AccountId = x.AccountId,
                                                        AccountName = x.AccountName,
                                                        Amount = x.Amount,
                                                        DateOperation = x.DateOperation,
                                                        CurrencyType = x.CurrencyType,
                                                        TypeOperation = x.TypeOperation
                                                    });

            return View(resultOperations);
        }
        public IActionResult RateOperations()
        {
            var thisAccounts = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userManager.GetUserId(User)));
            var transactions = unitOfWork.ExchangeRate.GetAll().Join(thisAccounts, x => x.AccountId, y => y.Id, (x, y) => x);

            var currencies = unitOfWork.Currency.GetAll();
            var statuses = unitOfWork.TransactionStatus.GetAll();

            var resultOperations = transactions
                                                    .Join(statuses, x => x.TransactionStatusId, y => y.Id, (x, y) => new
                                                    { x.AccountId, x.CurrencyId, x.Rate, x.Quantity, x.DateOperation, TypeOperation = y.Name })
                                                    .Join(thisAccounts, x => x.AccountId, y => y.Id, (x, y) => new
                                                    { x.AccountId, x.CurrencyId, x.Rate, x.Quantity, x.DateOperation, x.TypeOperation, AccountName = y.Name })
                                                    .Join(currencies, x => x.CurrencyId, y => y.Id, (x, y) => new
                                                    { x.AccountId, x.Rate, x.Quantity, x.DateOperation, x.TypeOperation, x.AccountName, CurrencyType = y.Name })
                                                    .Select(x => new RateOperationModel
                                                    {
                                                        AccountId = x.AccountId,
                                                        AccountName = x.AccountName,
                                                        DateOperation = x.DateOperation,
                                                        Quantity = x.Quantity,
                                                        Rate = x.Rate,
                                                        TypeOperation = x.TypeOperation
                                                    });

            return View(resultOperations);
        }
        #endregion
    }
}
