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

        public async Task<List<AccountTransaction>> MapToAccountTransactionsAsync(IEnumerable<StringAccountTransactionModel> models, long accountId, List<ErrorReportModel> errors)
        {
            var result = new List<AccountTransaction>();
            if (models is null || errors is null)
                return result;

            foreach (var i in models)
            {
                var dbEntity = new AccountTransaction
                {
                    AccountId = accountId,
                    DateOperation = Convert.ToDateTime(i.DateOperation, culture)
                };

                if (decimal.TryParse(i.Amount, out decimal dbEntityAmount))
                    dbEntity.Amount = dbEntityAmount;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.AccountTransactionError, ErrorValue = $"Не удалось преобразовать \'{i.Amount}\' - {i.DateOperation}" });
                    continue;
                }
                var curency = await unitOfWork.Currency.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Currency));
                if (curency != null)
                    dbEntity.CurrencyId = curency.Id;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.AccountTransactionError, ErrorValue = $"Не удалось найти тип валюты для \'{ i.Currency}\' - { i.DateOperation}" });
                    continue;
                }
                var status = await unitOfWork.TransactionStatus.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.TransactionStatus));
                if (status != null)
                    dbEntity.TransactionStatusId = status.Id;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.AccountTransactionError, ErrorValue = $"Не удалось найти статус для \'{ i.TransactionStatus}\' - { i.DateOperation}" });
                    continue;
                }

                result.Add(dbEntity);
            }

            return result;
        }
        public async Task<List<Comission>> MapToComissionsAsync(IEnumerable<StringComissionModel> models, long accountId, List<ErrorReportModel> errors)
        {
            var result = new List<Comission>();
            if (models is null || errors is null)
                return result;

            foreach (var i in models)
            {
                var dbEntity = new Comission
                {
                    AccountId = accountId,
                    DateOperation = Convert.ToDateTime(i.DateOperation, culture)
                };

                if (decimal.TryParse(i.Amount, out decimal dbEntityAmount))
                    dbEntity.Amount = dbEntityAmount;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.ComissionError, ErrorValue = $"Не удалось преобразовать \'{ i.Amount}\' - { i.DateOperation}" });
                    continue;
                }
                var curency = await unitOfWork.Currency.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Currency));
                if (curency != null)
                    dbEntity.CurrencyId = curency.Id;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.ComissionError, ErrorValue = $"Не удалось найти тип валюты для \'{ i.Currency}\' - { i.DateOperation}" });
                    continue;
                }
                var type = await unitOfWork.ComissionType.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Type));
                if (type != null)
                    dbEntity.ComissionTypeId = type.Id;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.ComissionError, ErrorValue = $"Не удалось найти тип комиссии для \'{ i.Type}\' - { i.DateOperation}" });
                    continue;
                }

                result.Add(dbEntity);
            }
            return result;
        }
        public async Task<List<Dividend>> MapToDividendsAsync(IEnumerable<StringDividendModel> models, long accountId, List<ErrorReportModel> errors)
        {
            var result = new List<Dividend>();
            if (models is null || errors is null)
                return result;

            foreach (var i in models)
            {
                var dbEntity = new Dividend
                {
                    AccountId = accountId,
                    DateOperation = Convert.ToDateTime(i.DateOperation, culture)
                };

                if (decimal.TryParse(i.Amount, out decimal dbEntityAmount))
                    dbEntity.Amount = dbEntityAmount;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.DividendError, ErrorValue = $"Не удалось преобразовать \'{ i.Amount}\' - {i.DateOperation}" });
                    continue;
                }
                var curency = await unitOfWork.Currency.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Currency));
                if (curency != null)
                    dbEntity.CurrencyId = curency.Id;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.DividendError, ErrorValue = $"Не удалось найти тип валюты для \'{ i.Currency}\' - {i.DateOperation}" });
                    continue;
                }
                var identifier = await unitOfWork.Isin.GetAll().FirstOrDefaultAsync(x => i.CompanyName.IndexOf(x.Name) >= 0);
                if (identifier != null)
                    dbEntity.IsinId = identifier.Id;
                else
                {
                    errors.Add(new ErrorReportModel
                    {
                        ErrorType = ParseErrorTypes.DividendError,
                        ErrorValue = $"Не удалось найти совпадение компании для \'{ i.CompanyName}\' - {i.DateOperation}"
                    });
                    continue;
                }

                result.Add(dbEntity);
            }

            return result;
        }
        public async Task<List<ExchangeRate>> MapToExchangeRatesAsync(IEnumerable<StringExchangeRateModel> models, long accountId, List<ErrorReportModel> errors)
        {
            var result = new List<ExchangeRate>();
            if (models is null || errors is null)
                return result;

            foreach (var i in models)
            {
                var dbEntity = new ExchangeRate
                {
                    AccountId = accountId,
                    DateOperation = Convert.ToDateTime(i.DateOperation, culture)
                };

                if (long.TryParse(i.Identifier, out long dbEntityIdentifier))
                    dbEntity.Identifier = dbEntityIdentifier;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.ExchangeRateError, ErrorValue = $"Не удалось преобразовать \'{ i.Identifier}\' - { i.DateOperation}" });
                    continue;
                }
                if (int.TryParse(i.Quantity, out int dbEntityQuantity))
                    dbEntity.Quantity = dbEntityQuantity;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.ExchangeRateError, ErrorValue = $"Не удалось преобразовать \'{ i.Quantity}\' - { i.DateOperation}" });
                    continue;
                }
                if (decimal.TryParse(i.Rate, out decimal dbEntityRate))
                    dbEntity.Rate = dbEntityRate;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.ExchangeRateError, ErrorValue = $"Не удалось преобразовать \'{ i.Rate}\' - { i.DateOperation}" });
                    continue;
                }
                var status = await unitOfWork.TransactionStatus.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.TransactionStatus));
                if (status != null)
                    dbEntity.TransactionStatusId = status.Id;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.ExchangeRateError, ErrorValue = $"Не удалось найти статус для \'{ i.TransactionStatus}\' - { i.DateOperation}" });
                    continue;
                }
                var curency = await unitOfWork.Currency.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Currency));
                if (curency != null)
                    dbEntity.CurrencyId = curency.Id;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.ExchangeRateError, ErrorValue = $"Не удалось найти тип валюты для \'{ i.Currency}\' - { i.DateOperation}" });
                    continue;
                }

                result.Add(dbEntity);
            }

            return result;
        }
        public async Task<List<StockTransaction>> MapToStockTransactionsAsync(IEnumerable<StringStockTransactionModel> models, long accountId, List<ErrorReportModel> errors)
        {
            var result = new List<StockTransaction>();
            if (models is null || errors is null)
                return result;

            foreach (var i in models)
            {
                var dbEntity = new StockTransaction
                {
                    AccountId = accountId,
                    DateOperation = Convert.ToDateTime(i.DateOperation, culture)
                };

                if (long.TryParse(i.Identifier, out long dbEntityIdentifier))
                    dbEntity.Identifier = dbEntityIdentifier;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.StockTransactionError, ErrorValue = $"Не удалось преобразовать \'{ i.Identifier}\' - { i.DateOperation}" });
                    continue;
                }
                if (int.TryParse(i.Quantity, out int dbEntityQuantity))
                    dbEntity.Quantity = dbEntityQuantity;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.StockTransactionError, ErrorValue = $"Не удалось преобразовать \'{ i.Quantity}\' - { i.DateOperation}" });
                    continue;
                }
                if (decimal.TryParse(i.Cost, out decimal dbEntityCost))
                    dbEntity.Cost = dbEntityCost;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.StockTransactionError, ErrorValue = $"Не удалось преобразовать \'{i.Cost}\' - {i.DateOperation}" });
                    continue;
                }
                var status = await unitOfWork.TransactionStatus.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.TransactionStatus));
                if (status != null)
                    dbEntity.TransactionStatusId = status.Id;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.StockTransactionError, ErrorValue = $"Не удалось найти статус для \'{i.TransactionStatus}\' - {i.DateOperation}" });
                    continue;
                }
                var curency = await unitOfWork.Currency.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Currency));
                if (curency != null)
                    dbEntity.CurrencyId = curency.Id;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.StockTransactionError, ErrorValue = $"Не удалось найти тип валюты для \'{i.Currency}\' - {i.DateOperation}" });
                    continue;
                }
                var exchange = await unitOfWork.Exchange.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Exchange));
                if (exchange != null)
                    dbEntity.ExchangeId = exchange.Id;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.StockTransactionError, ErrorValue = $"Не удалось найти тип биржи для \'{ i.Exchange}\' - { i.DateOperation}" });
                    continue;
                }
                var ticker = await unitOfWork.Ticker.GetAll().FirstOrDefaultAsync(x => x.Name.Equals(i.Ticker));
                if (ticker != null)
                    dbEntity.TickerId = ticker.Id;
                else
                {
                    errors.Add(new ErrorReportModel { ErrorType = ParseErrorTypes.StockTransactionError, ErrorValue = $"Не удалось найти компанию по тикеру \'{ i.Ticker}\' - { i.DateOperation}" });
                    continue;
                }

                result.Add(dbEntity);
            }
            return result;
        }
    }
}
