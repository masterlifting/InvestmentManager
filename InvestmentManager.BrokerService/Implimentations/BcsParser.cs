using InvestmentManager.BrokerService.Interfaces;
using InvestmentManager.BrokerService.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace InvestmentManager.BrokerService.Implimentations
{
    public class BcsParser : IBcsParser
    {
        const string statusBuy = "Покупка";
        const string statusSell = "Продажа";
        const string statusReceipt = "Пополнение";
        const string statusWithdraw = "Снятие";
        const string currencyUsd = "usd";
        const string currencyRub = "rub";
        const string columnNumber = "column1";
        private static int dealsId;    //Номер строки, с которой начинаются сделки
        private static int assetsId;   //Номер строки, с которой начинаются активы

        public FilterReportModel ParsePeriodReport(DataSet excelReport)
        {
            if (excelReport.Tables.Count == 0)
                throw new NullReferenceException("Отсутствуют данные для парсинга");

            var report = excelReport.Tables[0];

            string period = report.Select($"{columnNumber} = 'Период:'").FirstOrDefault().ItemArray.Where(x => x is string).ElementAt(1).ToString();
            string[] periods = period.Split(" ");

            bool isDateBegin = DateTime.TryParse(periods[1], out DateTime parsedDeteBegin);
            bool isDateEnd = DateTime.TryParse(periods[3], out DateTime parsedDeteEnd);

            if (!isDateBegin || !isDateEnd)
                throw new ArgumentException($"Не удалось получить даты из строки отчета: {period}");

            string account = report.Select($"{columnNumber} = 'Генеральное соглашение:'")
                                               .FirstOrDefault().ItemArray.Where(x => x is string).ElementAt(1).ToString();

            return new FilterReportModel
            {
                AccountName = account,
                DateBegin = parsedDeteBegin,
                DateEnd = parsedDeteEnd
            };
        }
        public StringReportModel ParseBcsReport(DataSet excelReport)
        {
            var model = new StringReportModel();

            if (excelReport.Tables.Count == 0)
                throw new NullReferenceException("Отсутствуют данные для парсинга");

            var report = excelReport.Tables[0];

            SetTableBorders(report);
            var (dateBegin, dateEnd) = ParsePeriod(report);

            model.DateBeginReport = dateBegin;
            model.DateEndReport = dateEnd;

            model.AccountId = ParseAccountName(report);
            model.Comissions = ParseComissions(report);
            model.StockTransactions = ParseStockTransactions(report);
            model.Dividends = ParseDividends(report);
            model.AccountTransactions = ParseAccountTransactions(report);
            model.ExchangeRates = ParseExchangeRates(report);

            return model;
        }

        static IEnumerable<StringExchangeRateModel> ParseExchangeRates(DataTable report)
        {
            var exchangeRates = new List<StringExchangeRateModel>();

            if (dealsId > 0)
            {
                for (int i = dealsId + 1; i < assetsId - 1; i++)
                {
                    if (report.Rows[i].ItemArray[1].ToString() == "2.3. Незавершенные сделки")
                    {
                        break;
                    }

                    if (report.Rows[i].ItemArray[1].ToString() == "Иностранная валюта")
                    {
                        string currency = report.Rows[i + 3].ItemArray[1].ToString();
                        int startId = i + 3;
                        int finishId = startId;

                        while (report.Rows[finishId].ItemArray[1].ToString() != $"Итого по {currency}:")
                        {
                            finishId++;
                        }

                        for (int j = startId + 1; j < finishId; j++)
                        {
                            string typeDeal = report.Rows[j].ItemArray[12].ToString();
                            var antiPattern = typeDeal.Split(' ')[0].IndexOf("Своп", StringComparison.OrdinalIgnoreCase);

                            if (antiPattern >= 0)
                                continue;

                            BuildExchangeRate(j, 4, 7);
                        }

                        string currency2 = report.Rows[finishId + 1].ItemArray[1].ToString();

                        if (!string.IsNullOrWhiteSpace(currency2))
                        {
                            int startId2 = finishId + 1;
                            int finishId2 = startId2;

                            while (report.Rows[finishId2].ItemArray[1].ToString() != $"Итого по {currency2}:")
                            {
                                finishId2++;
                            }

                            for (int j = startId2 + 1; j < finishId2; j++)
                            {
                                if (report.Rows[j].ItemArray[12].ToString() != "РПС")
                                    continue;

                                BuildExchangeRate(j, 4, 7);
                            }
                        }
                    }
                }
            }

            return exchangeRates;

            void BuildExchangeRate(int lineTable, int buyPosition, int sellPosition)
            {
                string quantityBuy = report.Rows[lineTable].ItemArray[5].ToString();
                string quantitySell = report.Rows[lineTable].ItemArray[8].ToString();
                string identifier = report.Rows[lineTable].ItemArray[2].ToString();
                string dateOperation = report.Rows[lineTable].ItemArray[1].ToString();

                bool buy = int.TryParse(quantityBuy, out _);
                bool sell = int.TryParse(quantitySell, out _);
                bool identNo = long.TryParse(identifier, out _);

                if (buy && identNo)
                    exchangeRates.Add(new StringExchangeRateModel
                    {
                        DateOperation = dateOperation,
                        Identifier = identifier,
                        Quantity = quantityBuy,
                        Rate = report.Rows[lineTable].ItemArray[buyPosition].ToString(),
                        TransactionStatus = statusBuy,
                        Currency = currencyUsd
                    });
                else if (sell && identNo)
                    exchangeRates.Add(new StringExchangeRateModel
                    {
                        DateOperation = dateOperation,
                        Identifier = identifier,
                        Quantity = quantitySell,
                        Rate = report.Rows[lineTable].ItemArray[sellPosition].ToString(),
                        TransactionStatus = statusSell,
                        Currency = currencyUsd
                    });

            }
        }
        static IEnumerable<StringAccountTransactionModel> ParseAccountTransactions(DataTable report)
        {
            var accountTransactions = new List<StringAccountTransactionModel>();

            for (int i = 0; i < assetsId; i++)
            {
                string currency = report.Rows[i].ItemArray[1].ToString();

                if (currency.Equals("USD"))
                    BuildAccountTransaction(i, currency);
                if (currency.Equals("Рубль"))
                    BuildAccountTransaction(i, currency);
            }

            return accountTransactions;

            void BuildAccountTransaction(int lineTable, string currency)
            {
                int startId = lineTable + 2;
                int finishId = startId;

                while (report.Rows[finishId].ItemArray[1].ToString() != $"Итого по валюте {currency}:")
                {
                    finishId++;
                }
                //переберем все строки найденого диапазона и добавим найденые позиции
                for (int j = startId; j < finishId; j++)
                {
                    string dateOperation = report.Rows[j].ItemArray[1].ToString();

                    if (report.Rows[j].ItemArray[2].ToString().Equals("Приход ДС"))
                        accountTransactions.Add(new StringAccountTransactionModel
                        {
                            DateOperation = dateOperation,
                            Amount = report.Rows[j].ItemArray[6].ToString(),
                            TransactionStatus = statusReceipt,
                            Currency = currency.Equals("USD") ? currencyUsd : currencyRub,
                        });
                    if (report.Rows[j].ItemArray[2].ToString().Equals("Вывод ДС"))
                        accountTransactions.Add(new StringAccountTransactionModel
                        {
                            DateOperation = dateOperation,
                            Amount = report.Rows[j].ItemArray[7].ToString(),
                            TransactionStatus = statusWithdraw,
                            Currency = currency.Equals("USD") ? currencyUsd : currencyRub
                        });
                }
            }
        }
        static IEnumerable<StringDividendModel> ParseDividends(DataTable report)
        {
            var dividends = new List<StringDividendModel>();

            for (int i = 0; i < assetsId; i++)
            {
                string currency = report.Rows[i].ItemArray[1].ToString();

                if (currency.Equals("USD"))
                    BuildDividends(i, currency);
                if (currency.Equals("Рубль"))
                    BuildDividends(i, currency);
            }

            return dividends;

            void BuildDividends(int lineTable, string currency)
            {
                //определим диапазон поиска
                int startId = lineTable + 2;
                int finishId = startId;

                while (!report.Rows[finishId].ItemArray[1].ToString().Equals($"Итого по валюте {currency}:"))
                {
                    finishId++;
                }
                //переберем все строки найденого диапазона и добавим найденые позиции
                for (int j = startId; j < finishId; j++)
                {
                    if (report.Rows[j].ItemArray[2].ToString().ToLower().IndexOf("дивиденд") >= 0)
                        dividends.Add(new StringDividendModel
                        {
                            CompanyName = report.Rows[j].ItemArray[14].ToString(),
                            DateOperation = report.Rows[j].ItemArray[1].ToString(),
                            Amount = report.Rows[j].ItemArray[6].ToString(),
                            Currency = currency.Equals("USD") ? currencyUsd : currencyRub
                        });
                }

            }
        }
        static IEnumerable<StringStockTransactionModel> ParseStockTransactions(DataTable report)
        {
            var stockTransactions = new List<StringStockTransactionModel>();

            if (dealsId > 0)
            {
                for (int i = dealsId + 1; i < assetsId - 1; i++)
                {
                    if (report.Rows[i].ItemArray[1].ToString() == "2.3. Незавершенные сделки")
                    {
                        break;
                    }
                    else if (report.Rows[i].ItemArray[6].ToString() == "ISIN:")
                    {
                        string ticker = report.Rows[i].ItemArray[1].ToString();
                        int startId = i;
                        int finishId = startId;

                        while (report.Rows[finishId].ItemArray[1].ToString() != $"Итого по {ticker}:")
                        {
                            finishId++;
                        }

                        for (int j = startId + 1; j < finishId; j++)
                        {

                            string quantityBuy = report.Rows[j].ItemArray[4].ToString();
                            string quantitySell = report.Rows[j].ItemArray[7].ToString();
                            string identifier = report.Rows[j].ItemArray[2].ToString().Trim();

                            string exchange = report.Rows[j].ItemArray[17].ToString();
                            string dateOperation = report.Rows[j].ItemArray[1].ToString();
                            string currency = report.Rows[j].ItemArray[10].ToString();

                            bool buy = int.TryParse(quantityBuy, out _);
                            bool sell = int.TryParse(quantitySell, out _);
                            bool identNo = long.TryParse(identifier, out _);

                            if (buy && identNo)
                                stockTransactions.Add(new StringStockTransactionModel
                                {
                                    Ticker = ticker,
                                    Exchange = exchange,
                                    DateOperation = dateOperation,
                                    Identifier = identifier,
                                    Quantity = quantityBuy,
                                    Cost = report.Rows[j].ItemArray[5].ToString(),
                                    TransactionStatus = statusBuy,
                                    Currency = currency.Equals("USD") ? currencyUsd : currencyRub
                                });
                            else if (sell && identNo)
                                stockTransactions.Add(new StringStockTransactionModel
                                {
                                    Ticker = ticker,
                                    Exchange = exchange,
                                    DateOperation = dateOperation,
                                    Identifier = identifier,
                                    Quantity = quantitySell,
                                    Cost = report.Rows[j].ItemArray[8].ToString(),
                                    TransactionStatus = statusSell,
                                    Currency = currency.Equals("USD") ? currencyUsd : currencyRub
                                });
                        }
                    }
                }
            }

            return stockTransactions;

        }
        static IEnumerable<StringComissionModel> ParseComissions(DataTable report)
        {
            var comissions = new List<StringComissionModel>();

            var thisComissions = report.Select($"{columnNumber} = '1.3. Удержанные сборы/штрафы (итоговые суммы):'").FirstOrDefault();
            if(thisComissions is null)
                thisComissions = report.Select($"{columnNumber} = '1.3. Начисленные сборы/штрафы (итоговые суммы):'").FirstOrDefault();

            int startId = report.Rows.IndexOf(thisComissions);

            // Ищу удержания
            if (startId > 0)
            {
                startId += 4;
                int finishId = startId;

                while (report.Rows[finishId].ItemArray[1].ToString() != $"Итого по валюте Рубль:")
                {
                    finishId++;
                }
                //Сначала добавим те типы удержаний, которые указаны в разделе Удержания этого отчета
                for (int i = startId; i < finishId; i++)
                {
                    string comissionName = report.Rows[i].ItemArray[1].ToString();
                    string comissionExchangeName = report.Rows[i].ItemArray[8].ToString();

                    for (int k = 0; k < startId; k++)
                    {
                        if (report.Rows[k].ItemArray[2].ToString().Equals(comissionName) && report.Rows[k].ItemArray[12].ToString().Equals(comissionExchangeName))
                        {
                            comissions.Add(new StringComissionModel
                            {
                                Type = comissionName,
                                DateOperation = report.Rows[k].ItemArray[1].ToString(),
                                Amount = report.Rows[k].ItemArray[7].ToString(),
                                Currency = currencyRub
                            });
                        }
                        else if (report.Rows[k].ItemArray[2].ToString().Equals(comissionName) && report.Rows[k].ItemArray[10].ToString().Equals(comissionExchangeName))
                        {
                            comissions.Add(new StringComissionModel
                            {
                                Type = comissionName,
                                DateOperation = report.Rows[k].ItemArray[1].ToString(),
                                Amount = report.Rows[k].ItemArray[7].ToString(),
                                Currency = currencyRub
                            });
                        }
                    }
                }
            }

            SetNdfl(comissions);

            return comissions;

            void SetNdfl(List<StringComissionModel> result)
            {
                var ndfl = report.Select($"{columnNumber} = '1.1. Движение денежных средств по совершенным сделкам:'").FirstOrDefault();
                int ndflStartId = report.Rows.IndexOf(ndfl);
                int ndflFinishId = ndflStartId;

                for (int i = ndflStartId; i < assetsId; i++)
                    if (report.Rows[i].ItemArray[1].ToString() == $"Итого по валюте Рубль:")
                        ndflFinishId = i;

                if (ndflStartId == ndflFinishId)
                    return;

                for (int i = ndflStartId; i < ndflFinishId; i++)
                {
                    string ndflName = report.Rows[i].ItemArray[2].ToString();

                    if (ndflName.Equals("НДФЛ"))
                    {
                        result.Add(new StringComissionModel
                        {
                            Type = ndflName,
                            DateOperation = report.Rows[i].ItemArray[1].ToString(),
                            Amount = report.Rows[i].ItemArray[7].ToString(),
                            Currency = currencyRub
                        });
                    }
                }
            }
        }

        static void SetTableBorders(DataTable report)
        {
            var deals = report.Select($"{columnNumber} = '2.1. Сделки:'").FirstOrDefault();
            dealsId = report.Rows.IndexOf(deals);
            var assets = report.Select($"{columnNumber} = '3. Активы:'").FirstOrDefault();
            assetsId = report.Rows.IndexOf(assets);
        }
        static (DateTime dateBegin, DateTime dateEnd) ParsePeriod(DataTable report)
        {
            string period = report.Select($"{columnNumber} = 'Период:'").FirstOrDefault().ItemArray.Where(x => x is string).ElementAt(1).ToString();
            string[] periods = period.Split(" ");

            bool isDateBegin = DateTime.TryParse(periods[1], out DateTime parsedDeteBegin);
            bool isDateEnd = DateTime.TryParse(periods[3], out DateTime parsedDeteEnd);

            if (!isDateBegin || !isDateEnd)
                throw new ArgumentException($"Не удалось получить даты из строки отчета: {period}");

            return (parsedDeteBegin, parsedDeteEnd);
        }
        static string ParseAccountName(DataTable report)
        {
            return report.Select($"{columnNumber} = 'Генеральное соглашение:'")
            .FirstOrDefault().ItemArray
            .Where(x => x is string)
            .ElementAt(1)
            .ToString();
        }
    }
}
