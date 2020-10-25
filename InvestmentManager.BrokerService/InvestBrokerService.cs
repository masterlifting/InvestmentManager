using InvestmentManager.BrokerService.Implimentations;
using InvestmentManager.BrokerService.Interfaces;
using InvestmentManager.BrokerService.Models;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
            var checkedReports = await CheckParsedReportsAsync(parsedReports, userId).ConfigureAwait(false);
            var filteredNewReports = FilterOutNewReports(checkedReports);

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

            var accounts = await unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userId)).ToListAsync().ConfigureAwait(false);

            if (!accounts.Any())
            {
                errors.Add(new ErrorReportModel
                {
                    ErrorType = ParseErrorTypes.AccountError,
                    ErrorValue = "Для получения данных необходимо добавить ваш номер соглашения из отчета",
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

                var entityReportModel = new EntityReportModel { AccountId = account.Id, AccountName = model.AccountId };
                try
                {
                    entityReportModel.AccountTransactions = await reportMapper.MapToAccountTransactionsAsync(model.AccountTransactions, account.Id, errors).ConfigureAwait(false);
                    entityReportModel.StockTransactions = await reportMapper.MapToStockTransactionsAsync(model.StockTransactions, account.Id, errors).ConfigureAwait(false);
                    entityReportModel.Dividends = await reportMapper.MapToDividendsAsync(model.Dividends, account.Id, errors).ConfigureAwait(false);
                    entityReportModel.Comissions = await reportMapper.MapToComissionsAsync(model.Comissions, account.Id, errors).ConfigureAwait(false);
                    entityReportModel.ExchangeRates = await reportMapper.MapToExchangeRatesAsync(model.ExchangeRates, account.Id, errors).ConfigureAwait(false);
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
        ResultBrokerReportModel FilterOutNewReports(ResultBrokerReportModel checkedReports)
        {
            var newReports = new List<EntityReportModel>();

            foreach (var i in checkedReports.Reports.GroupBy(x => new { x.AccountId, x.AccountName }))
            {
                var reportModel = new EntityReportModel { AccountId = i.Key.AccountId, AccountName = i.Key.AccountName };
                reportModel.AccountTransactions = reportFilter.GetNewTransactions(i.Select(x => x.AccountTransactions).Aggregate((x, y) => x.Union(y)), i.Key.AccountId);
                reportModel.StockTransactions = reportFilter.GetNewTransactions(i.Select(x => x.StockTransactions).Aggregate((x, y) => x.Union(y)), i.Key.AccountId);
                reportModel.Dividends = reportFilter.GetNewTransactions(i.Select(x => x.Dividends).Aggregate((x, y) => x.Union(y)), i.Key.AccountId);
                reportModel.Comissions = reportFilter.GetNewTransactions(i.Select(x => x.Comissions).Aggregate((x, y) => x.Union(y)), i.Key.AccountId);
                reportModel.ExchangeRates = reportFilter.GetNewTransactions(i.Select(x => x.ExchangeRates).Aggregate((x, y) => x.Union(y)), i.Key.AccountId);

                newReports.Add(reportModel);
            }

            checkedReports.Reports = newReports;
            return checkedReports;
        }
        #endregion
    }
}
