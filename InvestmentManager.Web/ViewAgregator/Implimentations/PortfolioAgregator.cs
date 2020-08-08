using InvestmentManager.Repository;
using InvestmentManager.Web.Models.PortfolioModels;
using InvestmentManager.Web.ViewAgregator.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;
using System.Linq;

namespace InvestmentManager.Web.ViewAgregator.Implimentations
{
    public class PortfolioAgregator : IPortfolioAgregator
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public PortfolioAgregator(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

        public ResultBrokerReportViewModel GetResultReportView(IEnumerable<PortfolioReportModel> models)
        {
            var result = new ResultBrokerReportViewModel();
            var resultAccountTransactions = new List<AccountTransactionViewModel>();
            var resultStockTransactions = new List<StockTransactionViewModel>();
            var resultDividends = new List<DividendViewModel>();
            var resultComissions = new List<ComissionViewModel>();
            var resultExchangeRates = new List<ExchangeRateViewModel>();

            var dbCurrencies = unitOfWork.Currency.GetAll();
            var dbComissionTypes = unitOfWork.ComissionType.GetAll();
            var dbTransactionStatuses = unitOfWork.TransactionStatus.GetAll();
            var dbCompanies = unitOfWork.Company.GetAll();
            var dbTickers = unitOfWork.Ticker.GetAll();
            var dbExchanges = unitOfWork.Exchange.GetAll();
            var dbIsins = unitOfWork.Isin.GetAll();

            foreach (var i in models)
            {
                foreach (var j in i.AccountTransactions
                    .Join(dbCurrencies, x => x.CurrencyId, y => y.Id, (x, y) => new { x.DateOperation, Currency = y.Name, x.Amount, x.TransactionStatusId })
                    .Join(dbTransactionStatuses, x => x.TransactionStatusId, y => y.Id, (x, y) => new AccountTransactionViewModel
                    {
                        AccountName = i.AccountName,
                        DateOperation = x.DateOperation,
                        Amount = x.Amount,
                        Currency = x.Currency,
                        TransactionStatus = y.Name
                    }))
                { resultAccountTransactions.Add(j); }

                foreach (var j in i.StockTransactions
                    .Join(dbCurrencies, x => x.CurrencyId, y => y.Id, (x, y) => new { x.DateOperation, Currency = y.Name, x.TickerId, x.Identifier, x.Quantity, x.Cost, x.TransactionStatusId, x.ExchangeId })
                    .Join(dbTickers, x => x.TickerId, y => y.Id, (x, y) => new { x.DateOperation, x.Currency, y.CompanyId, Ticker = y.Name, x.Identifier, x.Quantity, x.Cost, x.TransactionStatusId, x.ExchangeId })
                    .Join(dbExchanges, x => x.ExchangeId, y => y.Id, (x, y) => new { x.DateOperation, x.Currency, x.CompanyId, x.Ticker, x.Identifier, x.Quantity, x.Cost, x.TransactionStatusId, Exchange = y.Name })
                    .Join(dbCompanies, x => x.CompanyId, y => y.Id, (x, y) => new { x.DateOperation, x.Currency, Company = y.Name, x.Ticker, x.Identifier, x.Quantity, x.Cost, x.TransactionStatusId, x.Exchange })
                    .Join(dbTransactionStatuses, x => x.TransactionStatusId, y => y.Id, (x, y) => new StockTransactionViewModel
                    {
                        AccountName = i.AccountName,
                        DateOperation = x.DateOperation,
                        Currency = x.Currency,
                        CompanyName = x.Company,
                        Identifier = x.Identifier,
                        Quantity = x.Quantity,
                        Cost = x.Cost,
                        Ticker = x.Ticker,
                        Exchange = x.Exchange,
                        TransactionStatus = y.Name
                    }))
                { resultStockTransactions.Add(j); }

                foreach (var j in i.Dividends
                    .Join(dbCurrencies, x => x.CurrencyId, y => y.Id, (x, y) => new { x.DateOperation, Currency = y.Name, x.IsinId, x.Isin, x.Amount, x.Tax })
                    .Join(dbIsins, x => x.IsinId, y => y.Id, (x, y) => new { x.DateOperation, x.Currency, y.CompanyId, Isin = y.Name, x.Amount, x.Tax })
                    .Join(dbCompanies, x => x.CompanyId, y => y.Id, (x, y) => new DividendViewModel
                    {
                        AccountName = i.AccountName,
                        DateOperation = x.DateOperation,
                        Currency = x.Currency,
                        CompanyName = y.Name,
                        Amount = x.Amount,
                        Tax = x.Tax,
                        Isin = x.Isin
                    }))
                { resultDividends.Add(j); }

                foreach (var j in i.Comissions
                    .Join(dbCurrencies, x => x.CurrencyId, y => y.Id, (x, y) => new { x.DateOperation, Currency = y.Name, x.Amount, x.ComissionTypeId })
                    .Join(dbComissionTypes, x => x.ComissionTypeId, y => y.Id, (x, y) => new ComissionViewModel
                    {
                        AccountName = i.AccountName,
                        DateOperation = x.DateOperation,
                        Amount = x.Amount,
                        Currency = x.Currency,
                        ComissionTypeName = y.Name
                    }))
                { resultComissions.Add(j); }

                foreach (var j in i.ExchangeRates
                    .Join(dbCurrencies, x => x.CurrencyId, y => y.Id, (x, y) => new { x.DateOperation, Currency = y.Name, x.Quantity, x.Rate, x.TransactionStatusId, x.Identifier })
                    .Join(dbTransactionStatuses, x => x.TransactionStatusId, y => y.Id, (x, y) => new ExchangeRateViewModel
                    {
                        AccountName = i.AccountName,
                        DateOperation = x.DateOperation,
                        Currency = x.Currency,
                        Quantity = x.Quantity,
                        Rate = x.Rate,
                        Identifier = x.Identifier,
                        TransactionStatus = y.Name
                    }))
                { resultExchangeRates.Add(j); }
            }

            result.StockTransactions = resultStockTransactions.OrderBy(x => x.DateOperation);
            result.ExchangeRates = resultExchangeRates.OrderBy(x => x.DateOperation);
            result.AccountTransactions = resultAccountTransactions.OrderBy(x => x.DateOperation);
            result.Dividends = resultDividends.OrderBy(x => x.DateOperation);
            result.Comissions = resultComissions.OrderBy(x => x.DateOperation);

            return result;
        }
        public DividendErrorForm LoadDividendErrorForm()
        {
            var model = new DividendErrorForm();
            var companies = unitOfWork.Company.GetAll().OrderBy(x => x.Name);

            if (companies.Any())
            {
                model.Companies = new SelectList(companies, "Id", "Name");
                model.CompanyId = companies.First().Id;
            }

            return model;
        }
        public StockTransactionErrorForm LoadStockTransactionErrorForm()
        {
            var model = new StockTransactionErrorForm();

            var companies = unitOfWork.Company.GetAll().OrderBy(x => x.Name);
            var exchanges = unitOfWork.Exchange.GetAll();
            var lots = unitOfWork.Lot.GetAll();

            if (companies.Any())
            {
                model.Companies = new SelectList(companies, "Id", "Name");
                model.CompanyId = companies.First().Id;
            }

            if (exchanges.Any())
            {
                model.Exchanges = new SelectList(exchanges, "Id", "Name");
                model.ExchangeId = exchanges.First().Id;
            }

            if (lots.Any())
            {
                model.Lots = new SelectList(lots, "Id", "Value");
                model.LotId = lots.First().Id;
            }


            return model;
        }
    }
}
