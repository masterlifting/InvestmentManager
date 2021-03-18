using HtmlAgilityPack;
using InvestmentManager.Entities.Market;
using InvestmentManager.ReportFinder.Interfaces;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.ReportFinder.Implimentations
{
    public class InvestingAgregator : IReportAgregator
    {
        private readonly IWebService httpService;
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IConverterService converterService;
        const NumberStyles style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
        private static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("ru-RU");
        const string basePattern = "https://ru.investing.com/equities/";
        static (List<DateTime> NewDates, List<DateTime> FoundedDates) ParsedDates;

        public InvestingAgregator(
            IWebService httpService
            , IUnitOfWorkFactory unitOfWork
            , IConverterService converterService)
        {
            this.httpService = httpService;
            this.unitOfWork = unitOfWork;
            this.converterService = converterService;
        }
        public async Task<List<Report>> GetNewReportsAsync(long companyId, string sourceValue, object additional = null)
        {
            var resultReports = new List<Report>();

            var parsedReports = await ParserAsync(companyId, sourceValue);

            return parsedReports is null || (parsedReports.DecimalDictionary.Count == 0 && parsedReports.DividendCollection.Count == 0)
                ? resultReports
                : ConvertToReport(parsedReports, companyId);
        }


        private async Task<Dictionary<string, HtmlDocument>> ConfigureDataAsync(long companyId, string patternFromDb)
        {
            // Составлю базовый паттерн для запросов
            string pattern = $"{basePattern}{patternFromDb}";

            //Получаю страницу с данными отчетов для определения новых отчетов

            var financialSummaryQuery = $"{pattern}-financial-summary";
            var response = await httpService.GetDataAsync(financialSummaryQuery);

            var financialSummaryPage = new HtmlDocument();
            financialSummaryPage.LoadHtml(await response.Content.ReadAsStringAsync());

            if (financialSummaryPage is null)
                throw new NullReferenceException("Первая страница не загружена");

            //Сравниваю даты отчетов.");
            var lastDateReportFromDb = await unitOfWork.Report.GetLastFourDateReportAsync(companyId);

            if (!lastDateReportFromDb.Any())
            {
                //В базе по этой компании отчетов нет. Загружаю все
                ParsedDates = GetParsedDateFromInvesting(financialSummaryPage, lastDateReportFromDb);
                return await DownloadDataAsync(pattern, financialSummaryPage);
            }

            //Получаю даты новых отчетов и даты всех найденых отчетов
            ParsedDates = GetParsedDateFromInvesting(financialSummaryPage, lastDateReportFromDb);

            return ParsedDates.NewDates.Any() ? await DownloadDataAsync(pattern, financialSummaryPage) : new Dictionary<string, HtmlDocument>();
        }
        private async Task<Dictionary<string, HtmlDocument>> DownloadDataAsync(string pattern, HtmlDocument financialSummaryPage)
        {
            //Загружаю остальные страницы с данными
            var mainResponse = await httpService.GetDataAsync(pattern);
            var mainPage = new HtmlDocument();
            mainPage.LoadHtml(await mainResponse.Content.ReadAsStringAsync());
            if (mainPage is null)
                throw new NullReferenceException("Главная страница не была загружена");

            var balanceResponse = await httpService.GetDataAsync($"{pattern}-balance-sheet");
            var balancePage = new HtmlDocument();
            balancePage.LoadHtml(await balanceResponse.Content.ReadAsStringAsync());
            if (balancePage is null)
                throw new NullReferenceException("Страница с балансами не была загружена");

            var dividendsResponse = await httpService.GetDataAsync($"{pattern}-dividends");
            var dividendsPage = new HtmlDocument();
            dividendsPage.LoadHtml(await dividendsResponse.Content.ReadAsStringAsync());
            if (dividendsPage is null)
                throw new NullReferenceException("Страница с дивидендами не была загружена");

            //Все страницы загружены

            var resultPages = new Dictionary<string, HtmlDocument>
            {
                { "financialSummary", financialSummaryPage },
                { "mainPage", mainPage },
                { "balance", balancePage },
                { "dividends", dividendsPage }
            };

            return resultPages;
        }

        // Работаю с страницами сайта
        private async Task<ParsedDataResult> ParserAsync(long companyId, string sourceValue)
        {
            var pagesWithData = await ConfigureDataAsync(companyId, sourceValue);
            if (!pagesWithData.Any())
                return new ParsedDataResult();
            //Начинаю парсинг найденных данных.

            #region Создаю задачи на основе методов парсинга
            var stockCirculationTask = GetStockInCirculation(pagesWithData["mainPage"]);
            var revenueTask = GetFinancialSummaryValue(pagesWithData["financialSummary"], 0, "Общий доход");
            var grossProfitTask = GetFinancialSummaryValue(pagesWithData["financialSummary"], 0, "Валовая прибыль");
            var netProfitTask = GetFinancialSummaryValue(pagesWithData["financialSummary"], 0, "Чистая прибыль");
            var assetsTask = GetFinancialSummaryValue(pagesWithData["financialSummary"], 1, "Итого активы");
            var obligationsTask = GetFinancialSummaryValue(pagesWithData["financialSummary"], 1, "Итого обязательства");
            var shareCapitalTask = GetFinancialSummaryValue(pagesWithData["financialSummary"], 1, "Итого акционерный капитал");
            var cashFlowTask = GetFinancialSummaryValue(pagesWithData["financialSummary"], 2, "Чистое изменение денежных средств");
            var turnoverTask = GetTurnoverOrLongTermDebt(pagesWithData["balance"], "Итого оборотные активы");
            var longTermDebtTask = GetTurnoverOrLongTermDebt(pagesWithData["balance"], "Общая долгосрочная задолженность по кредитам и займам");
            var dividendCollectionTask = GetDividendCollection(pagesWithData["dividends"]);
            #endregion
            #region Выполняю эти задачи параллельно, где это возможно
            var listDecimalResult = await Task.WhenAll(new[]
            {
                revenueTask,
                grossProfitTask,
                netProfitTask,
                assetsTask,
                obligationsTask,
                shareCapitalTask,
                cashFlowTask,
                turnoverTask,
                longTermDebtTask });
            #endregion
            #region Собираю полученные данные и возвращаю для привидения к моим типам
            var listDividendResult = await dividendCollectionTask;
            var stockInCirculation = await stockCirculationTask;
            var decimalDictionary = new Dictionary<string, List<decimal>>
            {
                { "RevenueCollection", listDecimalResult[0] },
                { "GrossProfitCollection", listDecimalResult[1] },
                { "NetProfitCollection", listDecimalResult[2] },
                { "AssetsCollection", listDecimalResult[3] },
                { "ObligationCollection", listDecimalResult[4] },
                { "ShareCapitalCollection", listDecimalResult[5] },
                { "CashFlowCollection", listDecimalResult[6] },
                { "TurnoverCollection", listDecimalResult[7] },
                { "LongTermDebtCollection", listDecimalResult[8] }
            };

            //Парсинг окончен.");
            Console.ResetColor();


            return new ParsedDataResult
            {
                StockVolume = stockInCirculation,
                DecimalDictionary = decimalDictionary,
                DividendCollection = listDividendResult
            };
            #endregion
        }
        // Конвертирую полученные данные в свой тип
        private List<Report> ConvertToReport(ParsedDataResult dataResult, long companyId)
        {
            var resultReports = new List<Report>();
            var parsedReports = new List<Report>();

            for (int i = 0; i < ParsedDates.FoundedDates.Count; i++)
            {
                Report reportFromSite = new Report
                {
                    DateReport = ParsedDates.FoundedDates[i],
                    CompanyId = companyId,
                    StockVolume = dataResult.StockVolume,
                    Revenue = dataResult.DecimalDictionary["RevenueCollection"][i],
                    GrossProfit = dataResult.DecimalDictionary["GrossProfitCollection"][i],
                    NetProfit = dataResult.DecimalDictionary["NetProfitCollection"][i],
                    Assets = dataResult.DecimalDictionary["AssetsCollection"][i],
                    CashFlow = dataResult.DecimalDictionary["CashFlowCollection"][i],
                    ShareCapital = dataResult.DecimalDictionary["ShareCapitalCollection"][i],
                    Turnover = dataResult.DecimalDictionary["TurnoverCollection"][i],
                    LongTermDebt = dataResult.DecimalDictionary["LongTermDebtCollection"][i],
                    Obligations = dataResult.DecimalDictionary["ObligationCollection"][i]
                };

                dataResult.DividendCollection.TryGetValue((ParsedDates.FoundedDates[i].Year, converterService.ConvertToQuarter(ParsedDates.FoundedDates[i].Month)), out decimal dividend);
                reportFromSite.Dividends = dividend;

                parsedReports.Add(reportFromSite);
            }

            //Оставляем только новые отчеты
            foreach (var date in ParsedDates.NewDates)
            {
                resultReports.Add(parsedReports.FirstOrDefault(x => x.DateReport.Date == date.Date));
            }

            return resultReports;
        }
        // Метод получения дат отчетов, которых нет в моей БД с отчетами
        private (List<DateTime> NewDates, List<DateTime> FoundDates) GetParsedDateFromInvesting(HtmlDocument htmlPage, IEnumerable<DateTime> lastDateReportFromDb)
        {
            if (htmlPage is null)
                throw new NullReferenceException($"Html страница для работы с датами отчетов не поступила в метод {nameof(GetParsedDateFromInvesting)}");

            List<DateTime> foundDates = new();
            var dateNode = htmlPage.DocumentNode.SelectNodes("//th[@class='arial_11 noBold title right period']").FirstOrDefault();
            if (dateNode is not null)
            {
                var dates = dateNode.ParentNode.InnerText.Split("\n");
                for (int i = 0; i < dates.Length; i++)
                    if (DateTime.TryParse(dates[i], out DateTime date))
                        foundDates.Add(date);
            }

            var foundConvertedDates = new Dictionary<(int year, int quarter), DateTime>();
            foreach (var i in foundDates)
            {
                foundConvertedDates.Add((i.Year, converterService.ConvertToQuarter(i.Month)), i);
            }

            var dbConvertedDates = new Dictionary<(int year, int quarter), DateTime>();
            foreach (var i in lastDateReportFromDb)
            {
                dbConvertedDates.Add((i.Year, converterService.ConvertToQuarter(i.Month)), i);
            }

            //Даты отчетов, которых еще нет в БД
            var newConvertedDates = foundConvertedDates.Keys.Except(dbConvertedDates.Keys);

            var newDates = new List<DateTime>();

            foreach (var i in newConvertedDates)
            {
                newDates.Add(foundConvertedDates[i]);
            }

            return (newDates, foundDates);
        }

        #region Шаблонные методы, с помощью которых осуществляется парсинг
        //Возвращает количество акций в обращении
        private static async Task<long> GetStockInCirculation(HtmlDocument html)
        {
            long result = 0;

            if (html is null)
                return result;

            await Task.Run(() =>
            {
                var td = html.DocumentNode.SelectNodes("//dt").FirstOrDefault(x => x.InnerText == "Акции в обращении");

                if (td?.NextSibling is not null)
                {
                    string value = td.NextSibling.InnerText;
                    if (value is not null)
                    {
                        value = value.Replace(".", "");
                        _ = long.TryParse(value, out result);
                    }
                }
            });

            return result;
        }
        //Возвращает оборот и долгосрочную заадолженность
        private static async Task<List<decimal>> GetTurnoverOrLongTermDebt(HtmlDocument html, string pattern)
        {
            if (html is null) return new List<decimal>();

            var result = new List<decimal>();
            await Task.Run(() =>
            {
                result = html.DocumentNode
                    .SelectNodes("//span[@class]")
                    .Where(x => x.InnerText == pattern)
                    .FirstOrDefault()
                    .ParentNode.ParentNode.ChildNodes
                    .Where(x => x.Name == "td")
                    .Skip(1)
                    .Select(x => decimal.TryParse(x.InnerText, style, culture, out decimal value) ? value : 0).ToList();
            });
            return result;
        }
        //Возвращает Чистую прибыль || Валовую прибыль || Активы || Акционерный капитал || Обязательства || Денежный поток
        private static async Task<List<decimal>> GetFinancialSummaryValue(HtmlDocument html, int table, string pattern)
        {
            if (html is null) return new List<decimal>();

            var values = new List<decimal>();
            await Task.Run(() =>
            {
                var foundName = html.DocumentNode.SelectNodes("//table[@class='genTbl openTbl companyFinancialSummaryTbl']/tbody")[table]
                .ChildNodes.Where(x => x.Name == "tr" && x.InnerText.IndexOf(pattern, StringComparison.CurrentCultureIgnoreCase) > 0).FirstOrDefault();

                if (foundName != null)
                {
                    foreach (string value in foundName.ChildNodes.Where(x => x.Name == "td").Skip(1).Select(x => x.InnerText))
                    {
                        decimal.TryParse(value, style, culture, out decimal result);
                        values.Add(result);
                    }
                }
                else
                {
                    values.AddRange(new[] { 0m, 0m, 0m, 0m });
                }
            });

            return values;
        }
        //Возвращает видимую на странице коллекцию дивидендов от даты последнего отчета
        private async Task<Dictionary<(int year, int quarter), decimal>> GetDividendCollection(HtmlDocument html)
        {
            if (html is null) return new Dictionary<(int year, int quarter), decimal>();

            var collection = new Dictionary<(int year, int quarter), decimal>();

            var dividendQuery = html.DocumentNode.SelectNodes("//th[@class]").Where(x => x.InnerText == "Экс-дивиденд").FirstOrDefault();
            if (dividendQuery != null)
            {
                await Task.Run(() =>
                {
                    List<DateTime> dividentDate = dividendQuery.ParentNode.ParentNode.NextSibling.NextSibling.ChildNodes.Where(x => x.Name == "tr")
                        .Select(x => x.ChildNodes.Where(y => y.Name == "td")
                        .Select(z => z.InnerText))
                        .Select(x => x.FirstOrDefault())
                        .Select(x => DateTime.TryParse(x, out DateTime date) ? date : DateTime.MinValue).ToList();

                    List<decimal> dividendValue = dividendQuery.ParentNode.ParentNode.NextSibling.NextSibling.ChildNodes.Where(x => x.Name == "tr")
                        .Select(x => x.ChildNodes.Where(y => y.Name == "td")
                        .Select(z => z.InnerText))
                        .Select(x => x.Skip(1).FirstOrDefault())
                        .Select(x => decimal.TryParse(x.Replace(".", "", StringComparison.CurrentCultureIgnoreCase), style, culture, out decimal val) ? val : 0).ToList();

                    for (int i = 0; i < dividentDate.Count; i++)
                    {
                        if (i > 0 && ((dividentDate[i].Year, converterService.ConvertToQuarter(dividentDate[i].Month)) == (dividentDate[i - 1].Year, converterService.ConvertToQuarter(dividentDate[i - 1].Month))))
                        {
                            collection[(dividentDate[i - 1].Year, converterService.ConvertToQuarter(dividentDate[i - 1].Month))] += dividendValue[i];
                        }
                        else
                        {
                            collection.Add((dividentDate[i].Year, converterService.ConvertToQuarter(dividentDate[i].Month)), dividendValue[i]);
                        }
                    }
                });
            }

            return collection;
        }
        #endregion
    }
    internal class ParsedDataResult
    {
        internal long StockVolume { get; set; }
        internal Dictionary<(int year, int quarter), decimal> DividendCollection { get; set; }
        internal Dictionary<string, List<decimal>> DecimalDictionary { get; set; }
        internal ParsedDataResult()
        {
            DecimalDictionary = new Dictionary<string, List<decimal>>();
            DividendCollection = new Dictionary<(int year, int quarter), decimal>();
        }
    }
}
