using InvestmentManager.Calculator.ConfigurationBinding;
using InvestmentManager.Calculator.Implimentations;
using InvestmentManager.Calculator.Models;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Calculate;
using InvestmentManager.Entities.Market;
using InvestmentManager.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Calculator
{
    public class InvestCalculator : IInvestCalculator
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public InvestCalculator(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

        #region Coefficient
        public async Task<bool> SetCoeffitientsAsync(DataBaseType dbType)
        {
            var coefficients = new List<Coefficient>();

            #region Set data
            var reports = unitOfWork.Report.GetAll().Where(x => x.IsChecked).OrderBy(x => x.DateReport);
            var dateFirstReport = reports.First().DateReport;
            var dateLastReport = reports.Last().DateReport;
            var tikerData = unitOfWork.Price.GetAll()
                .Where(x => x.BidDate >= dateFirstReport && x.BidDate <= dateLastReport)
                .OrderByDescending(x => x.BidDate)
                .GroupBy(x => x.TickerId);
            var priceData = tikerData.Join(unitOfWork.Ticker.GetAll(), x => x.Key, y => y.Id, (x, y) => new { y.CompanyId, Prices = x });
            var coefficientData = await reports.Join(priceData, x => x.CompanyId, y => y.CompanyId, (x, y) =>
            new
            {
                Report = x,
                Price = y.Prices.Where(z => z.BidDate <= x.DateReport).First().Value
            }).ToArrayAsync().ConfigureAwait(false);
            #endregion

            try
            {
                for (int i = 0; i < coefficientData.Length; i++)
                {
                    var coefficient = CalculateReportCoefficient(coefficientData[i].Report, coefficientData[i].Price);
                    coefficients.Add(coefficient);
                }

                switch (dbType)
                {
                    case DataBaseType.Postgres:
                        unitOfWork.Coefficient.DeleteAndReseedPostgres();
                        break;
                    case DataBaseType.SQL:
                        unitOfWork.Coefficient.TruncateAndReseedSQL();
                        break;
                }
                await unitOfWork.Coefficient.CreateEntitiesAsync(coefficients).ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> SetCoeffitientAsync(Report report)
        {
            if (report is null || !report.IsChecked || report.Id == default)
                return false;

            int pricePeriod = ((DateTime.Now - report.DateReport).Days / 30) + 1;

            var prices = await unitOfWork.Price.GetCustomPricesAsync(report.CompanyId, pricePeriod, OrderType.OrderByDesc).ConfigureAwait(false);

            if (!prices.Any())
                return false;

            var price = prices.Where(x => x.BidDate <= report.DateReport).First().Value;
            var newCoefficient = CalculateReportCoefficient(report, price);

            var dbCoefficient = (await unitOfWork.Report.FindByIdAsync(report.Id).ConfigureAwait(false))?.Coefficient;

            if (dbCoefficient is null)
                await unitOfWork.Coefficient.CreateEntityAsync(newCoefficient).ConfigureAwait(false);
            else
            {
                dbCoefficient.DateUpdate = DateTime.Now;
                dbCoefficient.DebtLoad = newCoefficient.DebtLoad;
                dbCoefficient.EPS = newCoefficient.EPS;
                dbCoefficient.PB = newCoefficient.PB;
                dbCoefficient.PE = newCoefficient.PE;
                dbCoefficient.Profitability = newCoefficient.Profitability;
                dbCoefficient.ROA = newCoefficient.ROA;
                dbCoefficient.ROE = newCoefficient.ROE;
            }

            return true;
        }

        #region Helpers
        static Coefficient CalculateReportCoefficient(Report report, decimal lastPrice)
        {
            return report is not null && report.Id != default
                 ? new Coefficient() { ReportId = report.Id, PE = Pe(), PB = PB(), DebtLoad = DebtLoad(), Profitability = Profitability(), ROA = Roa(), ROE = Roe(), EPS = Eps() }
                 : throw new NullReferenceException("Нет отчета для расчета бизнес коэффициентов.");

            decimal Eps() => report.StockVolume != 0 ? ((report.NetProfit * 1_000_000) / report.StockVolume) : 0;
            decimal Profitability() => report.Revenue == 0 || report.Assets == 0 ? 0 : ((report.NetProfit / report.Revenue) + (report.Revenue / report.Assets)) / 2;
            decimal Roa() => report.Assets != 0 ? (report.NetProfit / report.Assets) * 100 : 0;
            decimal Roe() => report.ShareCapital != 0 ? (report.NetProfit / report.ShareCapital) * 100 : 0;
            decimal DebtLoad() => report.Assets != 0 ? report.Obligations / report.Assets : 0;
            decimal Pe() => Eps() != 0 && lastPrice > 0 ? lastPrice / Eps() : 0;
            decimal PB() => (report.Assets - report.Obligations) != 0 && lastPrice > 0 && report.StockVolume > 0 ? (lastPrice * report.StockVolume) / ((report.Assets - report.Obligations) * 1_000_000) : 0;
        }
        #endregion

        #endregion
        #region Rating
        public async Task<bool> SetRatingAsync(DataBaseType dbType)
        {
            var ratings = new List<Rating>();

            try
            {
                foreach (var company in unitOfWork.Company.GetAll())
                {
                    var rating = await CalculateRatingAsync(company).ConfigureAwait(false);

                    if (rating is not null)
                        ratings.Add(rating);
                    else return false;
                }

                SetRatingPlaces(ratings);

                switch (dbType)
                {
                    case DataBaseType.Postgres:
                        unitOfWork.Coefficient.DeleteAndReseedPostgres();
                        break;
                    case DataBaseType.SQL:
                        unitOfWork.Coefficient.TruncateAndReseedSQL();
                        break;
                }
                await unitOfWork.Rating.CreateEntitiesAsync(ratings).ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> SetRatingByPricesAsync()
        {
            var prices = unitOfWork.Price.GetGroupedPrices(12, OrderType.OrderBy);

            if (prices is null || !prices.Any())
                return false;

            var ratings = new List<Rating>();
            try
            {
                foreach (var data in unitOfWork.Company.GetAll().Join(prices, x => x.Id, y => y.Key, (x, y) => new { Company = x, Prices = y.Value }))
                {
                    Rating rating = data.Company.Rating is null ? new Rating { CompanyId = data.Company.Id } : data.Company.Rating;

                    if (!await SetRatingValueByPricesAsync(data.Prices, rating).ConfigureAwait(false))
                        continue;

                    SetRatingResult(rating);

                    ratings.Add(rating);
                }

                var currentRatings = await unitOfWork.Rating.GetAll().ToListAsync().ConfigureAwait(false);
                var notСalculatedRatings = currentRatings.Except(ratings).ToArray();
                for (int i = 0; i < notСalculatedRatings.Length; i++)
                {
                    notСalculatedRatings[i].PriceComparisonValue = null;
                    SetRatingResult(notСalculatedRatings[i]);
                }

                ratings.AddRange(notСalculatedRatings);

                SetRatingPlaces(ratings);

                await unitOfWork.CustomUpdateRangeAsync(ratings).ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> SetRatingByPricesAsync(long companyId)
        {
            var company = await unitOfWork.Company.FindByIdAsync(companyId).ConfigureAwait(false);

            if (company is null)
                return false;

            var prices = await unitOfWork.Price.GetCustomPricesAsync(company.Id, 12, OrderType.OrderBy, company.DateSplit).ConfigureAwait(false);

            if (prices is null || !prices.Any())
                return false;

            Rating rating = company.Rating is null ? new Rating { CompanyId = company.Id } : company.Rating;

            try
            {
                if (!await SetRatingValueByPricesAsync(prices.ToList(), rating).ConfigureAwait(false))
                    return false;

                SetRatingResult(rating);

                var ratings = await unitOfWork.Rating.GetAll().ToListAsync().ConfigureAwait(false);

                if (rating.Id != default)
                {
                    var removable = ratings.First(x => x.Id == rating.Id);
                    ratings.Remove(removable);
                    ratings.Add(rating);
                }
                else
                    ratings.Add(rating);

                SetRatingPlaces(ratings);

                await unitOfWork.CustomUpdateRangeAsync(ratings).ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> SetRatingByReportsAsync()
        {
            var reports = unitOfWork.Report.GetAll().Where(x => x.IsChecked).OrderBy(x => x.DateReport);

            if (reports is null || !reports.Any())
                return false;

            var ratings = new List<Rating>();
            var groupedReports = reports.GroupBy(x => x.CompanyId);
            try
            {
                foreach (var data in unitOfWork.Company.GetAll().Join(groupedReports, x => x.Id, y => y.Key, (x, y) => new { Company = x, Reports = y }))
                {
                    Rating rating = data.Company.Rating is null ? new Rating { CompanyId = data.Company.Id } : data.Company.Rating;

                    if (!await SetRatingValueByReportsAsync(data.Reports.ToList(), rating).ConfigureAwait(false))
                        continue;

                    SetRatingResult(rating);

                    ratings.Add(rating);
                }

                var currentRatings = await unitOfWork.Rating.GetAll().ToListAsync().ConfigureAwait(false);
                var notСalculatedRatings = currentRatings.Except(ratings).ToArray();
                for (int i = 0; i < notСalculatedRatings.Length; i++)
                {
                    notСalculatedRatings[i].ReportComparisonValue = null;
                    notСalculatedRatings[i].CashFlowPositiveBalanceValue = null;
                    SetRatingResult(notСalculatedRatings[i]);
                }

                ratings.AddRange(notСalculatedRatings);

                SetRatingPlaces(ratings);

                await unitOfWork.CustomUpdateRangeAsync(ratings).ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> SetRatingByReportsAsync(long companyId)
        {
            var company = await unitOfWork.Company.FindByIdAsync(companyId).ConfigureAwait(false);

            if (company is null)
                return false;

            var reports = company.Reports.Where(x => x.IsChecked).OrderBy(x => x.DateReport).ToList();

            if (reports is null || !reports.Any())
                return false;

            Rating rating = company.Rating is null ? new Rating { CompanyId = company.Id } : company.Rating;

            try
            {
                if (!await SetRatingValueByReportsAsync(reports, rating).ConfigureAwait(false))
                    return false;

                SetRatingResult(rating);

                var ratings = await unitOfWork.Rating.GetAll().ToListAsync().ConfigureAwait(false);

                if (rating.Id != default)
                {
                    var removable = ratings.First(x => x.Id == rating.Id);
                    ratings.Remove(removable);
                    ratings.Add(rating);
                }
                else
                    ratings.Add(rating);

                SetRatingPlaces(ratings);

                await unitOfWork.CustomUpdateRangeAsync(ratings).ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> SetRatingByCoefficientsAsync()
        {
            var coefficients = unitOfWork.Report.GetAll()
                .Where(x => x.IsChecked)
                .OrderBy(x => x.DateReport)
                .GroupBy(x => x.CompanyId)
                .Select(x => new { CompanyId = x.Key, Coefficients = x.Select(y => y.Coefficient) });

            if (coefficients is null || !coefficients.Any())
                return false;

            var ratings = new List<Rating>();
            try
            {
                foreach (var data in unitOfWork.Company.GetAll().Join(coefficients, x => x.Id, y => y.CompanyId, (x, y) => new { Company = x, y.Coefficients }))
                {
                    Rating rating = data.Company.Rating is null ? new Rating { CompanyId = data.Company.Id } : data.Company.Rating;

                    if (!await SetRatingValueByCoefficientsAsync(data.Coefficients.ToList(), rating).ConfigureAwait(false))
                        continue;

                    SetRatingResult(rating);

                    ratings.Add(rating);
                }

                var currentRatings = await unitOfWork.Rating.GetAll().ToListAsync().ConfigureAwait(false);
                var notСalculatedRatings = currentRatings.Except(ratings).ToArray();
                for (int i = 0; i < notСalculatedRatings.Length; i++)
                {
                    notСalculatedRatings[i].CoefficientAverageValue = null;
                    notСalculatedRatings[i].CoefficientComparisonValue = null;
                    SetRatingResult(notСalculatedRatings[i]);
                }

                ratings.AddRange(notСalculatedRatings);

                SetRatingPlaces(ratings);

                await unitOfWork.CustomUpdateRangeAsync(ratings).ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }

        }
        public async Task<bool> SetRatingByCoefficientsAsync(long companyId)
        {
            var company = await unitOfWork.Company.FindByIdAsync(companyId).ConfigureAwait(false);

            if (company is null)
                return false;

            var coefficients = company.Reports.Where(x => x.IsChecked).OrderBy(x => x.DateReport).Select(x => x.Coefficient).ToList();

            if (coefficients is null || !coefficients.Any())
                return false;

            Rating rating = company.Rating is null ? new Rating { CompanyId = company.Id } : company.Rating;
            try
            {
                if (!await SetRatingValueByCoefficientsAsync(coefficients, rating).ConfigureAwait(false))
                    return false;

                SetRatingResult(rating);

                var ratings = await unitOfWork.Rating.GetAll().ToListAsync().ConfigureAwait(false);

                if (rating.Id != default)
                {
                    var removable = ratings.First(x => x.Id == rating.Id);
                    ratings.Remove(removable);
                    ratings.Add(rating);
                }
                else
                    ratings.Add(rating);

                SetRatingPlaces(ratings);

                await unitOfWork.CustomUpdateRangeAsync(ratings).ConfigureAwait(false);

                return true;

            }
            catch
            {
                return false;
            }

        }

        #region Helpers
        static async Task<bool> SetRatingValueByPricesAsync(List<Price> prices, Rating rating)
        {
            if (rating is not null && prices is not null && prices.Any())
            {
                try
                {
                    rating.PriceComparisonValue = await Task.Run(() => new PriceCalculate(prices).GetPricieComporision()).ConfigureAwait(false);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else return false;
        }
        static async Task<bool> SetRatingValueByCoefficientsAsync(List<Coefficient> coefficients, Rating rating)
        {
            if (rating is not null && coefficients is not null && coefficients.Any())
            {
                try
                {
                    var coefficientCalculate = new CoefficientCalculate(coefficients);
                    rating.CoefficientComparisonValue = await Task.Run(() => coefficientCalculate.GetCoefficientComparison()).ConfigureAwait(false);
                    rating.CoefficientAverageValue = await Task.Run(() => coefficientCalculate.GetCoefficientAverage()).ConfigureAwait(false);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else return false;
        }
        static async Task<bool> SetRatingValueByReportsAsync(List<Report> reports, Rating rating)
        {
            if (rating is not null && reports is not null && reports.Any())
            {
                try
                {
                    var reportCalculate = new ReportCalculate(reports);
                    rating.ReportComparisonValue = await Task.Run(() => reportCalculate.GetReportComporision()).ConfigureAwait(false);
                    rating.CashFlowPositiveBalanceValue = await Task.Run(() => reportCalculate.GetCashFlowBalance()).ConfigureAwait(false);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else return false;
        }
        async Task<Rating> CalculateRatingAsync(Company company)
        {
            if (company is null)
                return null;

            #region Set data
            var prices = await unitOfWork.Price.GetCustomPricesAsync(company.Id, 12, OrderType.OrderBy, company.DateSplit).ConfigureAwait(false);
            var reports = company.Reports.Where(x => x.IsChecked).OrderBy(x => x.DateReport);
            var coefficients = reports.Select(x => x.Coefficient);
            #endregion

            var rating = new Rating { CompanyId = company.Id };

            if (!await SetRatingValueByPricesAsync(prices.ToList(), rating).ConfigureAwait(false)
                || !await SetRatingValueByCoefficientsAsync(coefficients.ToList(), rating).ConfigureAwait(false)
                || !await SetRatingValueByReportsAsync(reports.ToList(), rating).ConfigureAwait(false))
                return null;

            SetRatingResult(rating);

            return rating;
        }
        static void SetRatingResult(Rating rating)
        {
            // Вычисление результата
            var valuesByResult = new List<decimal>();

            if (rating.CashFlowPositiveBalanceValue.HasValue)
                valuesByResult.Add(rating.CashFlowPositiveBalanceValue.Value);
            if (rating.CoefficientAverageValue.HasValue)
                valuesByResult.Add(rating.CoefficientAverageValue.Value);
            if (rating.CoefficientComparisonValue.HasValue)
                valuesByResult.Add(rating.CoefficientComparisonValue.Value);
            if (rating.PriceComparisonValue.HasValue)
                valuesByResult.Add(rating.PriceComparisonValue.Value);
            if (rating.ReportComparisonValue.HasValue)
                valuesByResult.Add(rating.ReportComparisonValue.Value);

            rating.Result = valuesByResult.Any() ? valuesByResult.Average() : 0;
        }
        static void SetRatingPlaces(List<Rating> ratings)
        {
            ratings.Sort(new RatingCompare());

            for (int i = 1; i <= ratings.Count; i++)
            {
                ratings[i].Place = i;
                ratings[i].DateUpdate = DateTime.Now;
            }
        }
        #endregion

        #endregion

        BuyRecommendation CalculateBuyRecommendation(BuyRecommendationArgs args)
        {
            decimal deviationPercentage = BuyRecommendationConfig.DeviationPercentage;

            BuyRecommendation recommendation = new BuyRecommendation();
            if (args != null && args.Prices != null)
            {
                var prices = args.Prices.Where(x => x.Value > 0);
                decimal averagePriceList = prices.Any() ? prices.Average(x => x.Value) : 0;

                var recommendationPrice = averagePriceList > 0 ? averagePriceList * GetPercent(args.CompanyCountWhitPrice, args.RatingPlace) * 0.01m : 0;
                return (new BuyRecommendation
                {
                    CompanyId = args.CompanyId,
                    Price = recommendationPrice
                });
            }

            return recommendation;

            decimal GetPercent(int companyCountWhitPriceList, int rankInRating)
            {
                if (rankInRating == 0 || companyCountWhitPriceList == 0) return 0;

                decimal targetAllocationPercentMAX = 100;

                decimal targetAllocationPercentStep = (targetAllocationPercentMAX - deviationPercentage) / companyCountWhitPriceList;

                return targetAllocationPercentMAX - targetAllocationPercentStep * (rankInRating - 1);
            }
        }
        SellRecommendation CalculateSellRecommendation(SellRecommendationArgs args)
        {
            decimal lotMaxPercent = SellRecommendationConfig.LotMaxPercent > 0 ? SellRecommendationConfig.LotMaxPercent : 50;
            decimal lotMidPercent = SellRecommendationConfig.LotMidPercent > 0 ? SellRecommendationConfig.LotMidPercent : 30;
            decimal lotMinPercent = SellRecommendationConfig.LotMinPercent > 0 ? SellRecommendationConfig.LotMinPercent : 20;

            decimal profitMaxPercent = SellRecommendationConfig.ProfitMaxPercent > 0 ? SellRecommendationConfig.ProfitMaxPercent * 0.01m + 1 : 1.8m;
            decimal profitMidPercent = SellRecommendationConfig.ProfitMidPercent > 0 ? SellRecommendationConfig.ProfitMidPercent * 0.01m + 1 : 1.5m;
            decimal profitMinPercent = SellRecommendationConfig.ProfitMinPercent > 0 ? SellRecommendationConfig.ProfitMinPercent * 0.01m + 1 : 1.2m;

            SellRecommendation result = new SellRecommendation();

            if (args is null) return result;

            var operations = args.StockTransactions.OrderBy(x => x.DateOperation).Select(x => new { x.TransactionStatusId, x.Quantity, x.Cost });

            decimal freeAvgPrice = 0;
            int sellCount = 0;

            //всего купленых акций
            int stockCount = args.BuyValue / args.Lot;
            //Количество непроданных акций в портфеле
            int freeStockCount = (args.BuyValue - args.SellValue) / args.Lot;

            //получим цену, от которой будем устанавливать рекомендованные цены
            if (freeStockCount > 0)
            {
                var buyCountPrice = operations
                        .Where(x => x.TransactionStatusId == 3)
                        .OrderBy(x => x.Cost)
                        .Select(x => new { Quantity = x.Quantity / args.Lot, x.Cost })
                        .ToArray();

                int countStockMaxPrice = 0;
                int freeStockCountTemp = freeStockCount;
                int stockMaxPriceCountTemp = 0;
                decimal sumBuyMaxPrice = 0;

                int index = buyCountPrice.Length - 1;
                while (countStockMaxPrice < freeStockCount)
                {
                    countStockMaxPrice += buyCountPrice[index].Quantity;

                    if ((freeStockCountTemp - buyCountPrice[index].Quantity) > 0)
                    {
                        stockMaxPriceCountTemp = buyCountPrice[index].Quantity;
                        freeStockCountTemp -= buyCountPrice[index].Quantity;
                    }
                    else
                    {
                        stockMaxPriceCountTemp = freeStockCountTemp;
                    }
                    sumBuyMaxPrice += buyCountPrice[index].Cost * stockMaxPriceCountTemp;
                    index--;
                }
                freeAvgPrice = sumBuyMaxPrice / freeStockCount;

                var (percentMax, percentMid, percentMin) = GetPercents();

                //Разделим свободные акции по карманам в зависимости от распределения
                decimal lotMax = Math.Round((stockCount * percentMax) * 0.01m, 0);
                decimal lotMid = Math.Round((stockCount * percentMid) * 0.01m, 0);
                decimal lotMin = Math.Round((stockCount * percentMin) * 0.01m, 0);

                //Если распределилось не до конца, то дораспределим
                if ((lotMax + lotMid + lotMin) < freeStockCount)
                {
                    decimal a = stockCount - (lotMax + lotMid + lotMin);
                    decimal b = (new decimal[] { percentMin, percentMid, percentMax }).Max();
                    if (b == percentMax)
                        lotMax += a;
                    else if (b == percentMid)
                        lotMid += a;
                    else if (b == percentMin)
                        lotMin += a;
                }
                else if ((lotMax + lotMid + lotMin) > stockCount)
                {
                    decimal a = (lotMax + lotMid + lotMin) - stockCount;
                    decimal b = (new decimal[] { percentMin, percentMid, percentMax }).Min();
                    if (b == percentMax)
                        lotMax -= a;
                    else if (b == percentMid)
                        lotMid -= a;
                    else if (b == percentMin)
                        lotMin -= a;
                }
                else if (lotMax == 0 && lotMid == 0 && lotMin == 0 && percentMax > 0)
                {
                    lotMax = stockCount;
                }

                //Зададим цены к продаже в зависимости от необходимой прибыльности
                decimal priceMax = freeAvgPrice * profitMaxPercent;
                decimal priceMid = freeAvgPrice * profitMidPercent;
                decimal priceMin = freeAvgPrice * profitMinPercent;

                //Заполним модель
                result.CompanyId = args.CompanyId;
                result.LotMin = (int)lotMin;
                result.LotMid = (int)lotMid;
                result.LotMax = (int)lotMax;
                result.PriceMin = priceMin;
                result.PriceMid = priceMid;
                result.PriceMax = priceMax;

                //Вычтем с низа рекомендаций проданые лоты по порядку
                foreach (var operation in operations.Where(x => x.TransactionStatusId == 4))
                {
                    sellCount = operation.Quantity / args.Lot;

                    if (result.LotMin > 0 && sellCount > 0)
                    {
                        sellCount -= result.LotMin;
                        result.LotMin = sellCount < 0 ? Math.Abs(sellCount) : 0;
                    }
                    if (result.LotMid > 0 && sellCount > 0)
                    {
                        sellCount -= result.LotMid;
                        result.LotMid = sellCount < 0 ? Math.Abs(sellCount) : 0;
                    }
                    if (result.LotMax > 0 && sellCount > 0)
                    {
                        sellCount -= result.LotMax;
                        result.LotMax = sellCount < 0 ? Math.Abs(sellCount) : 0;
                    }
                }
            }
            return result;

            (decimal percentMax, decimal percentMid, decimal percentMin) GetPercents()
            {
                var result = (0m, 0m, 0m);

                if (args.RatingPlace > 0 && args.RatingCount > 0)
                {
                    (decimal maxPercentForBest, decimal midPercentForBest, decimal minPercentForBest) = (lotMaxPercent, lotMidPercent, lotMinPercent);
                    (decimal minPercentForBad, decimal midPercentForBad, decimal maxPercentForBad) = (0, 0, 100);

                    (decimal MAX, decimal MID, decimal MIN) = (
                    (maxPercentForBest - minPercentForBad) / args.RatingCount,
                    (midPercentForBest - midPercentForBad) / args.RatingCount,
                    (maxPercentForBad - minPercentForBest) / args.RatingCount);

                    result = (
                    maxPercentForBest - MAX * (args.RatingPlace - 1),
                    midPercentForBest - MID * (args.RatingPlace - 1),
                    minPercentForBest + MIN * (args.RatingPlace - 1));
                }
                return result;
            }

        }

        public List<SellRecommendation> GetCompleatedSellRecommendations(IEnumerable<string> userIds, IEnumerable<Rating> ratings)
        {
            var result = new List<SellRecommendation>();
            var accounts = unitOfWork.Account.GetAll();
            var stockTransactions = unitOfWork.StockTransaction.GetAll();
            var tickers = unitOfWork.Ticker.GetTickerIncludedLot();
            var companies = unitOfWork.Company.GetAll();
            int ratingCount = ratings.Count();

            foreach (var userId in userIds.ToList())
            {
                var userStockTransactions = stockTransactions.Join(accounts.Where(x => x.UserId.Equals(userId)), x => x.AccountId, y => y.Id, (x, y) => x);
                var companyIds = userStockTransactions.Join(tickers, x => x.TickerId, y => y.Id, (x, y) => y.CompanyId);

                foreach (var companyId in companyIds.Distinct().ToList())
                {
                    var companyTransactions = new List<StockTransaction>();
                    var recommendationArgs = new SellRecommendationArgs();
                    foreach (var ticker in tickers.Where(x => x.CompanyId == companyId).ToList())
                    {
                        companyTransactions.AddRange(userStockTransactions.Where(x => x.TickerId == ticker.Id));

                        if (recommendationArgs.Lot > 0 && recommendationArgs.Lot != ticker.Lot.Value)
                            throw new IndexOutOfRangeException("Один или более тикеров имеют разные лоты");

                        recommendationArgs.Lot = ticker.Lot.Value;
                    }
                    recommendationArgs.BuyValue = companyTransactions.Where(x => x.TransactionStatusId == 3).Sum(x => x.Quantity);
                    recommendationArgs.SellValue = companyTransactions.Where(x => x.TransactionStatusId == 4).Sum(x => x.Quantity);

                    if (recommendationArgs.BuyValue > recommendationArgs.SellValue)
                    {
                        recommendationArgs.RatingPlace = ratings.FirstOrDefault(x => x.CompanyId == companyId).Place;
                        recommendationArgs.RatingCount = ratingCount;
                        recommendationArgs.CompanyId = companyId;
                        recommendationArgs.StockTransactions = companyTransactions;
                        var sellRecommendation = CalculateSellRecommendation(recommendationArgs);
                        sellRecommendation.UserId = userId;
                        result.Add(sellRecommendation);
                    }
                }
            }
            return result;
        }
        public List<BuyRecommendation> GetCompleatedBuyRecommendations(IEnumerable<Rating> ratings)
        {
            var result = new List<BuyRecommendation>();
            int ratingCount = ratings.Count();
            int companyCountWithPrices = unitOfWork.Price.GetCompanyCountWithPrices();
            var prices = unitOfWork.Price.GetGroupedPricesByDateSplit(12, OrderType.OrderBy);

            foreach (var i in unitOfWork.Company.GetAll().AsEnumerable()
                .Join(prices, x => x.Id, y => y.Key, (x, y) => new { CompanyId = x.Id, Prices = y.Value })
                .Join(ratings, x => x.CompanyId, y => y.CompanyId, (x, y) => new { x.CompanyId, x.Prices, y.Place }))
            {
                var recommendationArgs = new BuyRecommendationArgs
                {
                    CompanyId = i.CompanyId,
                    Prices = i.Prices,
                    CompanyCountWhitPrice = companyCountWithPrices,
                    RatingPlace = i.Place
                };

                result.Add(CalculateBuyRecommendation(recommendationArgs));
            }
            return result;
        }

        public async Task SetBuyRecommendationsAsync()
        {
            throw new NotImplementedException();
        }
        public async Task SetSellRecommendationsAsync(string userId)
        {
            throw new NotImplementedException();
        }
        public async Task ResetCalculatingDataAsync(string userId)
        {
            var accountIds = await unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userId)).Select(x => x.Id).ToArrayAsync().ConfigureAwait(false);

        }

    }
    class RatingCompare : IComparer<Rating>
    {
        public int Compare(Rating x, Rating y) => x.Result < y.Result ? 1 : x.Result > y.Result ? -1 : 0;
    }
    public enum DataBaseType
    {
        Postgres,
        SQL
    }
}
