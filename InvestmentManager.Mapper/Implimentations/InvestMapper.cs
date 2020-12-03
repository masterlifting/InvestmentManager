using InvestmentManager.BrokerService.Models;
using InvestmentManager.Mapper.Interfaces;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.Services;
using System.Collections.Generic;
using System.Linq;
using static InvestmentManager.Models.Enums;

namespace InvestmentManager.Mapper.Implimentations
{
    public class InvestMapper : IInvestMapper
    {
        public BrokerReportModel MapBcsReports(ResultBrokerReportModel resultReportsModel)
        {
            var result = new BrokerReportModel();
            var successedReports = new List<BrokerReportSuccessedModel>();
            var reportErrors = new List<BrokerReportErrorModel>();

            foreach (var report in resultReportsModel.Reports)
            {
                var successedReport = new BrokerReportSuccessedModel { AccountId = report.AccountId };

                var accountTransactions = new List<AccountTransactionModel>();
                var stockTransactions = new List<StockTransactionModel>();
                var dividends = new List<DividendModel>();
                var comissions = new List<ComissionModel>();
                var exchangeRates = new List<ExchangeRateModel>();

                accountTransactions.AddRange(report.AccountTransactions.OrderBy(x => x.DateOperation).Select(x => new AccountTransactionModel
                {
                    AccountId = report.AccountId,
                    CurrencyId = x.CurrencyId,
                    StatusId = x.TransactionStatusId,
                    Amount = x.Amount,
                    DateOperation = x.DateOperation
                }));
                stockTransactions.AddRange(report.StockTransactions.OrderBy(x => x.DateOperation).Select(x => new StockTransactionModel
                {
                    AccountId = report.AccountId,
                    CurrencyId = x.CurrencyId,
                    ExchangeId = x.ExchangeId,
                    StatusId = x.TransactionStatusId,
                    TickerId = x.TickerId,
                    Cost = x.Cost,
                    Identifier = x.Identifier,
                    Quantity = x.Quantity,
                    DateOperation = x.DateOperation
                }));
                dividends.AddRange(report.Dividends.OrderBy(x => x.DateOperation).Select(x => new DividendModel
                {
                    AccountId = report.AccountId,
                    Amount = x.Amount,
                    CurrencyId = x.CurrencyId,
                    DateOperation = x.DateOperation,
                    IsinId = x.IsinId,
                    Tax = x.Tax
                }));
                comissions.AddRange(report.Comissions.OrderBy(x => x.DateOperation).Select(x => new ComissionModel
                {
                    AccountId = report.AccountId,
                    Amount = x.Amount,
                    CurrencyId = x.CurrencyId,
                    DateOperation = x.DateOperation,
                    TypeId = x.ComissionTypeId
                }));
                exchangeRates.AddRange(report.ExchangeRates.OrderBy(x => x.DateOperation).Select(x => new ExchangeRateModel
                {
                    AccountId = report.AccountId,
                    DateOperation = x.DateOperation,
                    CurrencyId = x.CurrencyId,
                    Identifier = x.Identifier,
                    Quantity = x.Quantity,
                    Rate = x.Rate,
                    StatusId = x.TransactionStatusId
                }));

                successedReport.AccountTransactions = accountTransactions;
                successedReport.StockTransactions = stockTransactions;
                successedReport.Dividends = dividends;
                successedReport.Comissions = comissions;
                successedReport.ExchangeRates = exchangeRates;

                successedReports.Add(successedReport);
            }
            foreach (var error in resultReportsModel.Errors.GroupBy(x => x.ErrorType))
            {
                var errorType = error.Key switch
                {
                    ParseErrorTypes.AccountError => BrokerReportErrorTypes.AccountError,
                    ParseErrorTypes.AccountTransactionError => BrokerReportErrorTypes.AccountTransactionError,
                    ParseErrorTypes.StockTransactionError => BrokerReportErrorTypes.StockTransactionError,
                    ParseErrorTypes.DividendError => BrokerReportErrorTypes.DividendError,
                    ParseErrorTypes.ComissionError => BrokerReportErrorTypes.ComissionError,
                    ParseErrorTypes.ExchangeRateError => BrokerReportErrorTypes.ExchangeRateError,
                    ParseErrorTypes.UndefinedError => BrokerReportErrorTypes.UndefinedError,
                    _ => throw new System.NotImplementedException()
                };

                reportErrors.AddRange(error.Select(x => new BrokerReportErrorModel
                {
                    ErrorType = errorType,
                    ErrorValue = x.ErrorValue
                }));
            }
            result.Reports = successedReports;
            result.Errors = reportErrors;
            return result;
        }
    }
}
