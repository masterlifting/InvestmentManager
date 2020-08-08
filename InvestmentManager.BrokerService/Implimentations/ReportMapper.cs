using InvestmentManager.BrokerService.Interfaces;
using InvestmentManager.BrokerService.Models;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace InvestmentManager.BrokerService.Implimentations
{
    public class ReportMapper : IReportMapper
    {
        static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("ru-RU");
        private readonly IUnitOfWorkFactory unitOfWork;
        public ReportMapper(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

        public async Task<List<AccountTransaction>> MapToAccountTransactionsAsync(IEnumerable<BrockerAccountTransactionModel> models, long accountId)
        {
            var result = new List<AccountTransaction>();
            if (models is null)
                return result;

            foreach (var i in models)
            {
                var dbEntity = new AccountTransaction
                {
                    AccountId = accountId,
                    // date operation
                    DateOperation = Convert.ToDateTime(i.DateOperation, culture)
                };

                // amount
                if (decimal.TryParse(i.Amount, out decimal dbEntityAmount))
                    dbEntity.Amount = dbEntityAmount;
                else throw new ArgumentException($"Не удалось преобразовать \'{i.Amount}\'");
                // curency
                var curency = await unitOfWork.Currency.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Currency)).ConfigureAwait(false);
                if (curency != null)
                    dbEntity.CurrencyId = curency.Id;
                else throw new NullReferenceException($"Не удалось найти тип валюты для \'{i.Currency}\'");
                // status
                var status = await unitOfWork.TransactionStatus.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.TransactionStatus));
                if (status != null)
                    dbEntity.TransactionStatusId = status.Id;
                else throw new NullReferenceException($"Не удалось найти статус для \'{i.TransactionStatus}\'");

                result.Add(dbEntity);
            }

            return result;
        }
        public async Task<List<Comission>> MapToComissionsAsync(IEnumerable<BrockerComissionModel> models, long accountId)
        {
            var result = new List<Comission>();
            if (models is null)
                return result;

            foreach (var i in models)
            {
                var dbEntity = new Comission
                {
                    AccountId = accountId,
                    // date operation
                    DateOperation = Convert.ToDateTime(i.DateOperation, culture)
                };

                // amount
                if (decimal.TryParse(i.Amount, out decimal dbEntityAmount))
                    dbEntity.Amount = dbEntityAmount;
                else throw new ArgumentException($"Не удалось преобразовать \'{i.Amount}\'");
                // curency
                var curency = await unitOfWork.Currency.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Currency));
                if (curency != null)
                    dbEntity.CurrencyId = curency.Id;
                else throw new NullReferenceException($"Не удалось найти тип валюты для \'{i.Currency}\'");
                // type
                var type = await unitOfWork.ComissionType.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Type));
                if (type != null)
                    dbEntity.ComissionTypeId = type.Id;
                else throw new NullReferenceException($"Не удалось найти тип комиссии для \'{i.Type}\'");

                result.Add(dbEntity);
            }
            return result;
        }
        public async Task<List<Dividend>> MapToDividendsAsync(IEnumerable<BrockerDividendModel> models, long accountId)
        {
            var result = new List<Dividend>();
            if (models is null)
                return result;

            foreach (var i in models)
            {
                var dbEntity = new Dividend
                {
                    AccountId = accountId,
                    // date operation
                    DateOperation = Convert.ToDateTime(i.DateOperation, culture)
                };

                // amount
                if (decimal.TryParse(i.Amount, out decimal dbEntityAmount))
                    dbEntity.Amount = dbEntityAmount;
                else throw new ArgumentException($"Не удалось преобразовать \'{i.Amount}\'");
                // curency
                var curency = await unitOfWork.Currency.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Currency));
                if (curency != null)
                    dbEntity.CurrencyId = curency.Id;
                else throw new NullReferenceException($"Не удалось найти тип валюты для \'{i.Currency}\'");
                // identifier
                var identifier = await unitOfWork.Isin.GetAll().FirstOrDefaultAsync(x => i.CompanyName.IndexOf(x.Name) >= 0).ConfigureAwait(false);
                if (identifier != null)
                    dbEntity.IsinId = identifier.Id;
                else throw new NullReferenceException($"Не удалось найти совпадение компании для \'{i.CompanyName}\'");

                result.Add(dbEntity);
            }

            return result;
        }
        public async Task<List<ExchangeRate>> MapToExchangeRatesAsync(IEnumerable<BrockerExchangeRateModel> models, long accountId)
        {
            var result = new List<ExchangeRate>();
            if (models is null)
                return result;

            foreach (var i in models)
            {
                var dbEntity = new ExchangeRate
                {
                    AccountId = accountId,
                    // date operation
                    DateOperation = Convert.ToDateTime(i.DateOperation, culture)
                };

                // identifier
                if (long.TryParse(i.Identifier, out long dbEntityIdentifier))
                    dbEntity.Identifier = dbEntityIdentifier;
                else throw new ArgumentException($"Не удалось преобразовать \'{i.Identifier}\'");
                // quantity
                if (int.TryParse(i.Quantity, out int dbEntityQuantity))
                    dbEntity.Quantity = dbEntityQuantity;
                else throw new ArgumentException($"Не удалось преобразовать \'{i.Quantity}\'");
                // rate
                if (decimal.TryParse(i.Rate, out decimal dbEntityRate))
                    dbEntity.Rate = dbEntityRate;
                else throw new ArgumentException($"Не удалось преобразовать \'{i.Rate}\'");
                // status
                var status = await unitOfWork.TransactionStatus.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.TransactionStatus));
                if (status != null)
                    dbEntity.TransactionStatusId = status.Id;
                else throw new NullReferenceException($"Не удалось найти статус для \'{i.TransactionStatus}\'");
                // curency
                var curency = await unitOfWork.Currency.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Currency));
                if (curency != null)
                    dbEntity.CurrencyId = curency.Id;
                else throw new NullReferenceException($"Не удалось найти тип валюты для \'{i.Currency}\'");

                result.Add(dbEntity);
            }

            return result;
        }
        public async Task<List<StockTransaction>> MapToStockTransactionsAsync(IEnumerable<BrockerStockTransactionModel> models, long accountId)
        {
            var result = new List<StockTransaction>();
            if (models is null)
                return result;

            foreach (var i in models)
            {
                var dbEntity = new StockTransaction
                {
                    AccountId = accountId,
                    // date operation
                    DateOperation = Convert.ToDateTime(i.DateOperation, culture)
                };

                // identifier
                if (long.TryParse(i.Identifier, out long dbEntityIdentifier))
                    dbEntity.Identifier = dbEntityIdentifier;
                else throw new ArgumentException($"Не удалось преобразовать \'{i.Identifier}\'");
                // quantity
                if (int.TryParse(i.Quantity, out int dbEntityQuantity))
                    dbEntity.Quantity = dbEntityQuantity;
                else throw new ArgumentException($"Не удалось преобразовать \'{i.Quantity}\'");
                // cost
                if (decimal.TryParse(i.Cost, out decimal dbEntityCost))
                    dbEntity.Cost = dbEntityCost;
                else throw new ArgumentException($"Не удалось преобразовать \'{i.Cost}\'");
                // status
                var status = await unitOfWork.TransactionStatus.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.TransactionStatus));
                if (status != null)
                    dbEntity.TransactionStatusId = status.Id;
                else throw new NullReferenceException($"Не удалось найти статус для \'{i.TransactionStatus}\'");
                // curency
                var curency = await unitOfWork.Currency.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Currency));
                if (curency != null)
                    dbEntity.CurrencyId = curency.Id;
                else throw new NullReferenceException($"Не удалось найти тип валюты для \'{i.Currency}\'");
                // exchange
                var exchange = await unitOfWork.Exchange.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Exchange));
                if (exchange != null)
                    dbEntity.ExchangeId = exchange.Id;
                else throw new NullReferenceException($"Не удалось найти тип биржи для \'{i.Exchange}\'");
                // ticker
                var ticker = await unitOfWork.Ticker.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Ticker)).ConfigureAwait(false);
                if (ticker != null)
                    dbEntity.TickerId = ticker.Id;
                else throw new NullReferenceException($"Не удалось найти компанию по тикеру \'{i.Ticker}\'");

                result.Add(dbEntity);
            }
            return result;
        }
    }
}
