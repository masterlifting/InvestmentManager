using InvestManager.BrokerService.Models;
using InvestManager.Mapper.Interfaces;
using InvestManager.Repository;
using InvestManager.ViewModels.PortfolioModels;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace InvestManager.Mapper.Implimentations
{
    public class PortfolioMapper : IPortfolioMapper
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public PortfolioMapper(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

        public BrokerReportModel MapBcsReports(ResultBrokerReportModel resultReportsModel)
        {
            var result = new BrokerReportModel();
            var correctReports = new List<CorrectBrokerReport>();
            var reportErrors = new List<BrokerReportError>();

            var transactionsStatusess = unitOfWork.TransactionStatus.GetAll().Select(x => new { x.Id, x.Name });
            var tickers = unitOfWork.Ticker.GetAll().Select(x => new { x.Name, x.Id, x.CompanyId });
            var exchanges = unitOfWork.Exchange.GetAll().Select(x => new { x.Name, x.Id });
            var companies = unitOfWork.Company.GetAll().Select(x => new { x.Id, x.Name });
            var isins = unitOfWork.Isin.GetAll().Select(x => new { x.CompanyId, x.Id });
            var comissionTypes = unitOfWork.ComissionType.GetAll().Select(x => new { x.Id, x.Name });

            foreach (var report in resultReportsModel.Reports)
            {
                var correctReport = new CorrectBrokerReport { AccountId = report.AccountName };
                var accountTransactions = new List<BrokerAccountTransaction>();
                var stockTransactions = new List<BrokerStockTransaction>();
                var dividends = new List<BrokerDividend>();
                var comissions = new List<BrokerComission>();
                var exchangeRates = new List<BrokerExchangeRate>();

                foreach (var i in report.AccountTransactions.OrderBy(x => x.DateOperation)
                            .Join(transactionsStatusess, x => x.TransactionStatusId, y => y.Id, (x, y) => new BrokerAccountTransaction
                            {
                                DateOperation = x.DateOperation.ToShortDateString(),
                                Amount = x.CurrencyId == 2 ? x.Amount.ToString("C") : x.Amount.ToString("C", new CultureInfo("en-US")),
                                Status = y.Name
                            }))
                    accountTransactions.Add(i);

                foreach (var i in report.StockTransactions.OrderBy(x => x.DateOperation)
                            .Join(transactionsStatusess, x => x.TransactionStatusId, y => y.Id, (x, y) => new { Transaction = x, Status = y.Name })
                            .Join(exchanges, x => x.Transaction.ExchangeId, y => y.Id, (x, y) => new { x.Transaction, x.Status, Exchange = y.Name })
                            .Join(tickers, x => x.Transaction.TickerId, y => y.Id, (x, y) => new { x.Transaction, x.Status, x.Exchange, y.CompanyId, Ticker = y.Name })
                            .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new BrokerStockTransaction
                            {
                                DateOperation = x.Transaction.DateOperation.ToShortDateString(),
                                Company = y.Name,
                                Cost = x.Transaction.CurrencyId == 2 ? x.Transaction.Cost.ToString("C") : x.Transaction.Cost.ToString("C", new CultureInfo("en-US")),
                                Exchange = x.Exchange,
                                Quantity = $"{x.Transaction.Quantity}",
                                Ticker = x.Ticker,
                                Status = x.Status
                            }))
                    stockTransactions.Add(i);


                foreach (var i in report.Dividends.OrderBy(x => x.DateOperation)
                            .Join(isins, x => x.IsinId, y => y.Id, (x, y) => new { Dividend = x, y.CompanyId })
                            .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new BrokerDividend
                            {
                                DateOperation = x.Dividend.DateOperation.ToShortDateString(),
                                Amount = x.Dividend.CurrencyId == 2 ? x.Dividend.Amount.ToString("C") : x.Dividend.Amount.ToString("C", new CultureInfo("en-US")),
                                Company = y.Name
                            }))
                    dividends.Add(i);


                foreach (var i in report.Comissions.OrderBy(x => x.DateOperation)
                            .Join(comissionTypes, x => x.ComissionTypeId, y => y.Id, (x, y) => new BrokerComission
                            {
                                DateOperation = x.DateOperation.ToShortDateString(),
                                Amount = x.CurrencyId == 2 ? x.Amount.ToString("C") : x.Amount.ToString("C", new CultureInfo("en-US")),
                                Type = y.Name
                            }))
                    comissions.Add(i);


                foreach (var i in report.ExchangeRates.OrderBy(x => x.DateOperation)
                            .Join(transactionsStatusess, x => x.TransactionStatusId, y => y.Id, (x, y) => new BrokerExchangeRate
                            {
                                DateOperation = x.DateOperation.ToShortDateString(),
                                Status = y.Name,
                                Quantity = x.Quantity.ToString("C", new CultureInfo("en-US")),
                                Rate = x.Rate.ToString("C")
                            }))
                    exchangeRates.Add(i);

                correctReport.AccountTransactions = accountTransactions;
                correctReport.StockTransactions = stockTransactions;
                correctReport.Dividends = dividends;
                correctReport.Comissions = comissions;
                correctReport.ExchangeRates = exchangeRates;

                correctReports.Add(correctReport);
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

                foreach (var i in error.Select(x => x))
                    reportErrors.Add(new BrokerReportError { ErrorType = errorType, ErrorValue = i.ErrorValue });
            }
            result.CorrectReports = correctReports;
            result.ReportErrors = reportErrors;
            return result;
        }
    }
}
