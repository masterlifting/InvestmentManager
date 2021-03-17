using InvestmentManager.BrokerService.Implimentations;
using InvestmentManager.BrokerService.Interfaces;
using InvestmentManager.BrokerService.Models;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InvestmentManager.BrokerService
{
    public class InvestBrokerService : IInvestBrokerService
    {
        private readonly IIOService iOService;
        private readonly IUnitOfWorkFactory unitOfWork;

        private readonly IBcsParser bcsParser;
        private readonly IReportFilter reportFilter;
        private readonly IReportMapper reportMapper;

        public InvestBrokerService(InvestmentContext context, IUnitOfWorkFactory unitOfWork, IIOService iOService)
        {
            this.unitOfWork = unitOfWork;
            this.iOService = iOService;
            bcsParser = new BcsParser();
            reportFilter = new ReportFilter(context);
            reportMapper = new ReportMapper(unitOfWork);
        }

        public async Task<ResultBrokerReportModel> GetNewReportsAsync(IFormFileCollection files, string userId)
        {
            if (files is null)
                throw new NullReferenceException();

            var parsedReports = ParseFiles(files);
            var checkedReports = await CheckParsedReportsAsync(parsedReports, userId);
            var filteredNewReports = await FilterOutNewReportsAsync(checkedReports);

            return filteredNewReports;
        }

        #region Include Logic
        List<StringReportModel> ParseFiles(IFormFileCollection files)
        {
            var stringReportModels = new List<StringReportModel>();
            var filteredReports = new Dictionary<FilterReportModel, DataSet>();

            foreach (var file in files)
            {
                Match match = Regex.Match(Path.GetFileNameWithoutExtension(file.FileName), @"B_k(.+)_ALL(.+)", RegexOptions.IgnoreCase);
                if (match.Success && Path.GetExtension(file.FileName).Equals(".xls"))
                {
                    using Stream stream = file.OpenReadStream();
                    using DataSet dataset = iOService.GetDataSet(stream);
                    filteredReports.Add(bcsParser.ParsePeriodReport(dataset), dataset);
                    stream.Close();
                }
            }

            if (!filteredReports.Any())
                return stringReportModels;

            var uniqueReports = reportFilter.GetUniqueLoadedReports(filteredReports.Keys);

            foreach (var bcsReport in uniqueReports)
                stringReportModels.Add(bcsParser.ParseBcsReport(filteredReports[bcsReport]));

            return stringReportModels;
        }
        async Task<ResultBrokerReportModel> CheckParsedReportsAsync(IEnumerable<StringReportModel> stringReportModels, string userId)
        {
            var result = new ResultBrokerReportModel();
            var reports = new List<EntityReportModel>();
            var errors = new List<ErrorReportModel>();

            var accounts = await unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userId)).ToListAsync();

            if (!accounts.Any())
            {
                errors.Add(new ErrorReportModel
                {
                    ErrorType = ParseErrorTypes.AccountError,
                    ErrorValue = $"Для получения данных необходимо добавить ваш номер соглашения из отчета '{stringReportModels.FirstOrDefault().AccountId}'",
                });

                result.Errors = errors;
                return result;
            }

            foreach (var model in stringReportModels)
            {
                var account = accounts.FirstOrDefault(x => x.Name.Equals(model.AccountId, StringComparison.OrdinalIgnoreCase));

                if (account is null)
                {
                    errors.Add(new ErrorReportModel
                    {
                        ErrorType = ParseErrorTypes.AccountError,
                        ErrorValue = $@"Добавьте номер соглашения '{model.AccountId}'"
                    });

                    result.Errors = errors;
                    continue;
                }

                var entityReportModel = new EntityReportModel { AccountId = account.Id };
                try
                {
                    entityReportModel.AccountTransactions = await reportMapper.MapToAccountTransactionsAsync(model.AccountTransactions, account.Id, errors);
                    entityReportModel.StockTransactions = await reportMapper.MapToStockTransactionsAsync(model.StockTransactions, account.Id, errors);
                    entityReportModel.Dividends = await reportMapper.MapToDividendsAsync(model.Dividends, account.Id, errors);
                    entityReportModel.Comissions = await reportMapper.MapToComissionsAsync(model.Comissions, account.Id, errors);
                    entityReportModel.ExchangeRates = await reportMapper.MapToExchangeRatesAsync(model.ExchangeRates, account.Id, errors);
                }
                catch (Exception ex)
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.UndefinedError, ErrorValue = ex.Message });
                }
                finally
                {
                    reports.Add(entityReportModel);
                }
            }

            result.Reports = reports;
            result.Errors = errors;

            return result;
        }
        async Task<ResultBrokerReportModel> FilterOutNewReportsAsync(ResultBrokerReportModel checkedReports)
        {
            var newReports = new List<EntityReportModel>();

            foreach (var i in checkedReports.Reports.GroupBy(x => x.AccountId))
            {
                var reportModel = new EntityReportModel { AccountId = i.Key };
                reportModel.AccountTransactions = await reportFilter.GetNewTransactionsAsync(i.Select(x => x.AccountTransactions).Aggregate((x, y) => x.Union(y)), i.Key, CheckAccountTransactions);
                reportModel.StockTransactions = await reportFilter.GetNewTransactionsAsync(i.Select(x => x.StockTransactions).Aggregate((x, y) => x.Union(y)), i.Key, CheckStockTransactions);
                reportModel.Dividends = await reportFilter.GetNewTransactionsAsync(i.Select(x => x.Dividends).Aggregate((x, y) => x.Union(y)), i.Key, CheckDividends);
                reportModel.Comissions = await reportFilter.GetNewTransactionsAsync(i.Select(x => x.Comissions).Aggregate((x, y) => x.Union(y)), i.Key, CheckComissions);
                reportModel.ExchangeRates = await reportFilter.GetNewTransactionsAsync(i.Select(x => x.ExchangeRates).Aggregate((x, y) => x.Union(y)), i.Key, CheckExchangeRates);

                newReports.Add(reportModel);
            }

            checkedReports.Reports = newReports;
            return checkedReports;

            #region Filter helpers
            static List<AccountTransaction> CheckAccountTransactions(List<AccountTransaction> incomeOperations, AccountTransaction[] dbOperations)
            {
                var result = new List<AccountTransaction>();

                for (int i = 0; i < incomeOperations.Count; i++)
                {
                    int comparisionResult = 0;

                    for (int j = 0; j < dbOperations.Length; j++)
                    {
                        if (incomeOperations[i].Amount == dbOperations[j].Amount
                            && incomeOperations[i].TransactionStatusId == dbOperations[j].TransactionStatusId
                            && incomeOperations[i].CurrencyId == dbOperations[j].CurrencyId)
                        {
                            int incomeOperationsCount = incomeOperations
                                .Where(x => x.Amount == incomeOperations[i].Amount
                                && x.TransactionStatusId == incomeOperations[i].TransactionStatusId
                                && x.CurrencyId == incomeOperations[i].CurrencyId)
                                .Count();

                            int dbOperationsCount = dbOperations
                                .Where(x => x.Amount == incomeOperations[i].Amount
                                && x.TransactionStatusId == incomeOperations[i].TransactionStatusId
                                && x.CurrencyId == incomeOperations[i].CurrencyId)
                                .Count();

                            int resultNewCount = incomeOperationsCount - dbOperationsCount;

                            for (int k = 0; k < resultNewCount; k++)
                            {
                                result.Add(incomeOperations[i]);
                                incomeOperations.RemoveAt(i);
                                i--;
                            }

                            comparisionResult = -1;
                            break;
                        }
                    }

                    if (comparisionResult == 0)
                        result.Add(incomeOperations[i]);
                }

                return result;
            }
            static List<StockTransaction> CheckStockTransactions(List<StockTransaction> incomeOperations, StockTransaction[] dbOperations)
            {
                var result = new List<StockTransaction>();

                for (int i = 0; i < incomeOperations.Count; i++)
                {
                    int comparisionResult = 0;

                    for (int j = 0; j < dbOperations.Length; j++)
                    {
                        if (incomeOperations[i].Identifier == dbOperations[j].Identifier)
                        {
                            comparisionResult = -1;
                            break;
                        }
                    }

                    if (comparisionResult == 0)
                        result.Add(incomeOperations[i]);
                }

                return result;
            }
            static List<Dividend> CheckDividends(List<Dividend> incomeOperations, Dividend[] dbOperations)
            {
                var result = new List<Dividend>();

                for (int i = 0; i < incomeOperations.Count; i++)
                {
                    int comparisionResult = 0;

                    for (int j = 0; j < dbOperations.Length; j++)
                    {
                        if (incomeOperations[i].Amount == dbOperations[j].Amount
                           && incomeOperations[i].Tax == dbOperations[j].Tax
                           && incomeOperations[i].IsinId == dbOperations[j].IsinId
                           && incomeOperations[i].CurrencyId == dbOperations[j].CurrencyId)
                        {
                            int incomeOperationsCount = incomeOperations
                               .Where(x => x.Amount == incomeOperations[i].Amount
                               && x.Tax == incomeOperations[i].Tax
                               && incomeOperations[i].IsinId == dbOperations[j].IsinId
                               && x.CurrencyId == incomeOperations[i].CurrencyId)
                               .Count();

                            int dbOperationsCount = dbOperations
                                .Where(x => x.Amount == incomeOperations[i].Amount
                                && x.Tax == incomeOperations[i].Tax
                                && incomeOperations[i].IsinId == dbOperations[j].IsinId
                                && x.CurrencyId == incomeOperations[i].CurrencyId)
                                .Count();

                            int resultNewCount = incomeOperationsCount - dbOperationsCount;

                            for (int k = 0; k < resultNewCount; k++)
                            {
                                result.Add(incomeOperations[i]);
                                incomeOperations.RemoveAt(i);
                                i--;
                            }

                            comparisionResult = -1;
                            break;
                        }
                    }

                    if (comparisionResult == 0)
                        result.Add(incomeOperations[i]);
                }

                return result;
            }
            static List<Comission> CheckComissions(List<Comission> incomeOperations, Comission[] dbOperations)
            {
                var result = new List<Comission>();

                for (int i = 0; i < incomeOperations.Count; i++)
                {
                    int comparisionResult = 0;

                    for (int j = 0; j < dbOperations.Length; j++)
                    {
                        if (incomeOperations[i].Amount == dbOperations[j].Amount
                            && incomeOperations[i].ComissionTypeId == dbOperations[j].ComissionTypeId
                            && incomeOperations[i].CurrencyId == dbOperations[j].CurrencyId)
                        {
                            int incomeOperationsCount = incomeOperations
                               .Where(x => x.Amount == incomeOperations[i].Amount
                               && x.ComissionTypeId == incomeOperations[i].ComissionTypeId
                               && x.CurrencyId == incomeOperations[i].CurrencyId)
                               .Count();

                            int dbOperationsCount = dbOperations
                                .Where(x => x.Amount == incomeOperations[i].Amount
                                && x.ComissionTypeId == incomeOperations[i].ComissionTypeId
                                && x.CurrencyId == incomeOperations[i].CurrencyId)
                                .Count();

                            int resultNewCount = incomeOperationsCount - dbOperationsCount;

                            for (int k = 0; k < resultNewCount; k++)
                            {
                                result.Add(incomeOperations[i]);
                                incomeOperations.RemoveAt(i);
                                i--;
                            }

                            comparisionResult = -1;
                            break;
                        }
                    }

                    if (comparisionResult == 0)
                        result.Add(incomeOperations[i]);
                }

                return result;
            }
            static List<ExchangeRate> CheckExchangeRates(List<ExchangeRate> incomeOperations, ExchangeRate[] dbOperations)
            {
                var result = new List<ExchangeRate>();

                for (int i = 0; i < incomeOperations.Count; i++)
                {
                    int comparisionResult = 0;

                    for (int j = 0; j < dbOperations.Length; j++)
                    {
                        if (incomeOperations[i].Identifier == dbOperations[j].Identifier)
                        {
                            comparisionResult = -1;
                            break;
                        }
                    }

                    if (comparisionResult == 0)
                        result.Add(incomeOperations[i]);
                }

                return result;
            }
            #endregion
        }
        #endregion
    }
}
