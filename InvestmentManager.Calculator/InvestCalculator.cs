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
using static InvestmentManager.Models.Enums;

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
            var prices = await unitOfWork.Price.GetAll().Where(x => x.BidDate >= dateFirstReport && x.BidDate <= dateLastReport).OrderByDescending(x => x.BidDate).ToArrayAsync();
            var companyData = await unitOfWork.Company.GetAll().Select(x => new { CompanyId = x.Id, TickerId = x.Tickers.First().Id }).ToArrayAsync();

            var groupedReports = (await reports.ToArrayAsync()).GroupBy(x => x.CompanyId);
            var groupedPrices = prices.Join(companyData, x => x.TickerId, y => y.TickerId, (x, y) => new { y.CompanyId, Price = x }).GroupBy(x => x.CompanyId);

            var coefficientData = groupedReports.Join(groupedPrices, x => x.Key, y => y.Key, (x, y) => new { Reports = x.ToArray(), Prices = y.Select(z => z.Price) }).ToArray();
            #endregion

            try
            {
                for (int i = 0; i < coefficientData.Length; i++)
                    for (int j = 0; j < coefficientData[i].Reports.Length; j++)
                    {
                        var report = coefficientData[i].Reports[j];
                        var price = coefficientData[i].Prices.FirstOrDefault(x => x.BidDate <= report.DateReport)?.Value;

                        if (!price.HasValue)
                            price = coefficientData[i].Prices.Last().Value;

                        var coefficient = CalculateReportCoefficient(report, price.Value);

                        if (coefficient is not null)
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

                await unitOfWork.Coefficient.CreateEntitiesAsync(coefficients);
                return await unitOfWork.CompleteAsync();
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

            var prices = await unitOfWork.Price.GetCustomOrderedPricesAsync(report.CompanyId, pricePeriod);

            if (!prices.Any())
                return false;

            var price = prices.Where(x => x.BidDate <= report.DateReport).Last().Value;
            var newCoefficient = CalculateReportCoefficient(report, price);

            var dbCoefficient = (await unitOfWork.Report.FindByIdAsync(report.Id))?.Coefficient;

            if (dbCoefficient is null)
                await unitOfWork.Coefficient.CreateEntityAsync(newCoefficient);
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

            return await unitOfWork.CompleteAsync();
        }

        #region Helpers
        static Coefficient CalculateReportCoefficient(Report report, decimal lastPrice)
        {
            if (report is null || report.Id == default || lastPrice == default)
                return null;

            decimal eps = report.StockVolume != 0 ? ((report.NetProfit * 1_000_000) / report.StockVolume) : 0;
            decimal profitability = report.Revenue == 0 || report.Assets == 0 ? 0 : ((report.NetProfit / report.Revenue) + (report.Revenue / report.Assets)) / 2;
            decimal roa = report.Assets != 0 ? (report.NetProfit / report.Assets) * 100 : 0;
            decimal roe = report.ShareCapital != 0 ? (report.NetProfit / report.ShareCapital) * 100 : 0;
            decimal debtLoad = report.Assets != 0 ? report.Obligations / report.Assets : 0;
            decimal pe = eps != 0 && lastPrice > 0 ? lastPrice / eps : 0;
            decimal pb = (report.Assets - report.Obligations) != 0 && lastPrice > 0 && report.StockVolume > 0 ? (lastPrice * report.StockVolume) / ((report.Assets - report.Obligations) * 1_000_000) : 0;

            return new Coefficient()
            {
                ReportId = report.Id,
                PE = pe,
                PB = pb,
                DebtLoad = debtLoad,
                Profitability = profitability,
                ROA = roa,
                ROE = roe,
                EPS = eps
            };
        }
        #endregion

        #endregion

        #region Rating
        public async Task<bool> SetRatingAsync(DataBaseType dbType)
        {
            var ratings = new List<Rating>();

            try
            {
                foreach (var companyId in await unitOfWork.Company.GetAll().Select(x => x.Id).ToArrayAsync())
                {
                    var rating = await CalculateRatingAsync(companyId);

                    if (rating is not null)
                        ratings.Add(rating);
                    else return false;
                }

                SetRatingPlaces(ratings);

                switch (dbType)
                {
                    case DataBaseType.Postgres:
                        unitOfWork.Rating.DeleteAndReseedPostgres();
                        break;
                    case DataBaseType.SQL:
                        unitOfWork.Rating.TruncateAndReseedSQL();
                        break;
                }

                await unitOfWork.Rating.CreateEntitiesAsync(ratings);
                return await unitOfWork.CompleteAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetRatingByPricesAsync()
        {
            var prices = await unitOfWork.Price.GetGroupedOrderedPricesAsync(12);

            if (prices is null || !prices.Any())
                return false;

            var ratings = new List<Rating>();
            try
            {
                foreach (var data in prices)
                {
                    var rating = unitOfWork.Rating.GetAll().FirstOrDefault(x => x.CompanyId == data.Key);
                    if (rating is null)
                        rating = new Rating { CompanyId = data.Key };

                    if (!await SetRatingValueByPricesAsync(data.Value, rating))
                        continue;

                    SetRatingResult(rating);

                    ratings.Add(rating);
                }

                var currentRatings = await unitOfWork.Rating.GetAll().ToListAsync();
                var notСalculatedRatings = currentRatings.Except(ratings).ToArray();
                for (int i = 0; i < notСalculatedRatings.Length; i++)
                {
                    notСalculatedRatings[i].PriceComparisonValue = null;
                    SetRatingResult(notСalculatedRatings[i]);
                }

                ratings.AddRange(notСalculatedRatings);

                SetRatingPlaces(ratings);

                return await unitOfWork.CustomUpdateRangeAsync(ratings);
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> SetRatingByPricesAsync(long companyId)
        {
            var company = await unitOfWork.Company.FindByIdAsync(companyId);

            if (company is null)
                return false;

            var prices = await unitOfWork.Price.GetCustomOrderedPricesAsync(company.Id, 12, company.DateSplit);

            if (prices is null || !prices.Any())
                return false;

            Rating rating = company.Rating is null ? new Rating { CompanyId = company.Id } : company.Rating;

            try
            {
                if (!await SetRatingValueByPricesAsync(prices, rating))
                    return false;

                SetRatingResult(rating);

                var ratings = await unitOfWork.Rating.GetAll().ToListAsync();

                if (rating.Id != default)
                    ratings.Remove(ratings.First(x => x.Id == rating.Id));

                ratings.Add(rating);
                SetRatingPlaces(ratings);

                return await unitOfWork.CustomUpdateRangeAsync(ratings);
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

                    var companyReports = data.Reports.Select(x => x);
                    var companyCoefficients = data.Reports.Select(x => x.Coefficient);

                    if (!await SetRatingValueByReportsAsync(companyReports, rating)
                    || !await SetRatingValueByCoefficientsAsync(companyCoefficients, rating))
                        continue;

                    SetRatingResult(rating);

                    ratings.Add(rating);
                }

                var currentRatings = await unitOfWork.Rating.GetAll().ToListAsync();
                var notСalculatedRatings = currentRatings.Except(ratings).ToArray();
                for (int i = 0; i < notСalculatedRatings.Length; i++)
                {
                    notСalculatedRatings[i].ReportComparisonValue = null;
                    notСalculatedRatings[i].CashFlowPositiveBalanceValue = null;
                    notСalculatedRatings[i].CoefficientComparisonValue = null;
                    notСalculatedRatings[i].CoefficientAverageValue = null;
                    SetRatingResult(notСalculatedRatings[i]);
                }

                ratings.AddRange(notСalculatedRatings);

                SetRatingPlaces(ratings);

                return await unitOfWork.CustomUpdateRangeAsync(ratings);
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> SetRatingByReportsAsync(long companyId)
        {
            var company = await unitOfWork.Company.FindByIdAsync(companyId);

            if (company is null)
                return false;

            var reports = company.Reports.Where(x => x.IsChecked).OrderBy(x => x.DateReport).ToList();
            var coefficients = reports.Join(unitOfWork.Coefficient.GetAll(), x => x.Id, y => y.ReportId, (x, y) => y);

            if (reports is null || !reports.Any())
                return false;

            Rating rating = company.Rating is null ? new Rating { CompanyId = company.Id } : company.Rating;

            try
            {
                if (!await SetRatingValueByReportsAsync(reports, rating)
                    || !await SetRatingValueByCoefficientsAsync(coefficients, rating))
                    return false;

                SetRatingResult(rating);

                var ratings = await unitOfWork.Rating.GetAll().ToListAsync();

                if (rating.Id != default)
                    ratings.Remove(ratings.First(x => x.Id == rating.Id));

                ratings.Add(rating);

                SetRatingPlaces(ratings);

                return await unitOfWork.CustomUpdateRangeAsync(ratings);
            }
            catch
            {
                return false;
            }
        }

        #region Helpers
        static async Task<bool> SetRatingValueByPricesAsync(IEnumerable<Price> prices, Rating rating)
        {
            if (rating is not null && prices is not null && prices.Any())
            {
                try
                {
                    rating.PriceComparisonValue = await Task.Run(() => new PriceCalculate(prices).GetPricieComporision());
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else return true;
        }
        static async Task<bool> SetRatingValueByCoefficientsAsync(IEnumerable<Coefficient> coefficients, Rating rating)
        {
            if (rating is not null && coefficients is not null && coefficients.Any())
            {
                try
                {
                    var coefficientCalculate = new CoefficientCalculate(coefficients);
                    rating.CoefficientComparisonValue = await Task.Run(() => coefficientCalculate.GetCoefficientComparison());
                    rating.CoefficientAverageValue = await Task.Run(() => coefficientCalculate.GetCoefficientAverage());
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else return true;
        }
        static async Task<bool> SetRatingValueByReportsAsync(IEnumerable<Report> reports, Rating rating)
        {
            if (rating is not null && reports is not null && reports.Any())
            {
                try
                {
                    var reportCalculate = new ReportCalculate(reports);
                    rating.ReportComparisonValue = await Task.Run(() => reportCalculate.GetReportComporision());
                    rating.CashFlowPositiveBalanceValue = await Task.Run(() => reportCalculate.GetCashFlowBalance());
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else return true;
        }
        async Task<Rating> CalculateRatingAsync(long companyId)
        {
            var company = await unitOfWork.Company.FindByIdAsync(companyId);

            if (company is null)
                return null;

            #region Set data
            var prices = await unitOfWork.Price.GetCustomOrderedPricesAsync(companyId, 12, company.DateSplit);
            var reports = company.Reports.Where(x => x.IsChecked).OrderBy(x => x.DateReport).ToList();
            var coefficients = reports.Join(unitOfWork.Coefficient.GetAll(), x => x.Id, y => y.ReportId, (x, y) => y);
            #endregion

            var rating = new Rating { CompanyId = company.Id };

            if (!await SetRatingValueByPricesAsync(prices, rating)
                || !await SetRatingValueByReportsAsync(reports, rating)
                || !await SetRatingValueByCoefficientsAsync(coefficients, rating))
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
            ratings.Sort((x, y) => x.Result < y.Result ? 1 : x.Result > y.Result ? -1 : 0);

            for (int i = 0; i < ratings.Count; i++)
            {
                ratings[i].Place = i + 1;
                ratings[i].DateUpdate = DateTime.Now;
            }
        }
        #endregion

        #endregion

        #region recommendations for buy
        public async Task<bool> SetBuyRecommendationsAsync(DataBaseType dbType)
        {
            var ratings = await unitOfWork.Rating.GetAll().ToArrayAsync();

            if (ratings is null || !ratings.Any())
                return false;

            var recommendations = new List<BuyRecommendation>();

            int companyCountWithPrices = await unitOfWork.Price.GetCompanyCountWithPricesAsync();
            var prices = await unitOfWork.Price.GetGroupedOrderedPricesAsync(12);

            foreach (var i in prices.Join(ratings, x => x.Key, y => y.CompanyId, (x, y) => new { Prices = x.Value, y.CompanyId, y.Place }))
            {
                var recommendationArgs = new BuyRecommendationArgs
                {
                    CompanyId = i.CompanyId,
                    Prices = i.Prices,
                    CompanyCountWhitPrice = companyCountWithPrices,
                    RatingPlace = i.Place
                };

                var recommendation = CalculateBuyRecommendation(recommendationArgs);

                if (recommendation is not null)
                    recommendations.Add(recommendation);
                else
                    continue;
            }

            try
            {
                switch (dbType)
                {
                    case DataBaseType.Postgres:
                        unitOfWork.BuyRecommendation.DeleteAndReseedPostgres();
                        break;
                    case DataBaseType.SQL:
                        unitOfWork.BuyRecommendation.TruncateAndReseedSQL();
                        break;
                }

                await unitOfWork.BuyRecommendation.CreateEntitiesAsync(recommendations);
                return await unitOfWork.CompleteAsync();
            }
            catch
            {
                return false;
            }
        }

        #region helpers
        static BuyRecommendation CalculateBuyRecommendation(BuyRecommendationArgs args)
        {
            decimal deviationPercentage = BuyRecommendationConfig.DeviationPercentage;

            if (args?.Prices is not null)
            {
                decimal priceAverage = args.Prices.Where(x => x.Value > 0).Average(x => x.Value);

                if (priceAverage == 0)
                    return null;

                decimal recommendationPrice = priceAverage * GetPercentByBuy(args.CompanyCountWhitPrice, args.RatingPlace) * 0.01m;

                return recommendationPrice != 0 ? new BuyRecommendation { CompanyId = args.CompanyId, Price = recommendationPrice } : null;
            }
            else
                return null;

            decimal GetPercentByBuy(int companyCountWhitPrices, int ratingPlace)
            {
                if (ratingPlace == 0 || companyCountWhitPrices == 0)
                    return 0;

                decimal targetAllocationPercentMAX = 100;

                decimal targetAllocationPercentStep = (targetAllocationPercentMAX - deviationPercentage) / companyCountWhitPrices;

                return targetAllocationPercentMAX - (targetAllocationPercentStep * (ratingPlace - 1));
            }
        }
        #endregion

        #endregion

        #region recommendations for sale
        public async Task<bool> SetSellRecommendationsAsync(DataBaseType dbType, string[] userIds)
        {
            #region set data
            var accounts = unitOfWork.Account.GetAll();
            var stockTransactions = unitOfWork.StockTransaction.GetAll();
            var tickers = unitOfWork.Ticker.GetAll();
            var ratings = await unitOfWork.Rating.GetAll().ToArrayAsync();
            #endregion

            var result = new List<SellRecommendation>();

            foreach (string userId in userIds)
            {
                var accountIds = await accounts.Where(x => x.UserId.Equals(userId)).Select(x => x.Id).ToArrayAsync();
                var userStockTransactions = await stockTransactions.Where(x => accountIds.Contains(x.AccountId)).ToArrayAsync();
                var recommendations = GetRecommendations(userId, tickers, ratings, userStockTransactions);
                if (recommendations.Any())
                    result.AddRange(recommendations);
            }

            try
            {
                switch (dbType)
                {
                    case DataBaseType.Postgres:
                        unitOfWork.SellRecommendation.DeleteAndReseedPostgres();
                        break;
                    case DataBaseType.SQL:
                        unitOfWork.SellRecommendation.TruncateAndReseedSQL();
                        break;
                }

                await unitOfWork.SellRecommendation.CreateEntitiesAsync(result);
                return await unitOfWork.CompleteAsync();
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> SetSellRecommendationAsync(string userId, long tickerId)
        {
            #region set data
            var accounts = unitOfWork.Account.GetAll();
            var stockTransactions = unitOfWork.StockTransaction.GetAll();
            var ratings = await unitOfWork.Rating.GetAll().ToArrayAsync();
            long companyId = (await unitOfWork.Ticker.FindByIdAsync(tickerId)).CompanyId;
            var companyTickerIds = (await unitOfWork.Company.FindByIdAsync(companyId)).Tickers.Select(x => x.Id);
            var lotValues = await unitOfWork.Ticker.GetAll().Where(x => companyTickerIds.Contains(x.Id)).Select(x => x.Lot.Value).ToArrayAsync();
            #endregion

            for (int i = 1; i < lotValues.Length; i++)
                if (lotValues[i - 1] != lotValues[i])
                    return false;

            var recommendation = await unitOfWork.SellRecommendation.GetAll().FirstOrDefaultAsync(x => x.UserId.Equals(userId) && x.CompanyId == companyId);

            var accountIds = await accounts.Where(x => x.UserId.Equals(userId)).Select(x => x.Id).ToArrayAsync();
            var companyStockTransactions = await stockTransactions.Where(x => accountIds.Contains(x.AccountId) && companyTickerIds.Contains(x.TickerId)).ToArrayAsync();

            var updatedRecommendation = GetRecommendation(userId, companyId, lotValues.First(), ratings, companyStockTransactions);

            if (updatedRecommendation is null)
                unitOfWork.SellRecommendation.DeleteEntity(recommendation);
            else if (recommendation is null)
                await unitOfWork.SellRecommendation.CreateEntityAsync(updatedRecommendation);
            else
            {
                updatedRecommendation.DateUpdate = DateTime.Now;
                unitOfWork.SellRecommendation.UpdateEntity(updatedRecommendation);
            }

            return await unitOfWork.CompleteAsync();
        }

        #region helpers
        static bool CalculateSellRecommendation(SellRecommendationArgs args, SellRecommendation result)
        {
            decimal lotMaxPercent = SellRecommendationConfig.LotMaxPercent > 0 ? SellRecommendationConfig.LotMaxPercent : 50;
            decimal lotMidPercent = SellRecommendationConfig.LotMidPercent > 0 ? SellRecommendationConfig.LotMidPercent : 30;
            decimal lotMinPercent = SellRecommendationConfig.LotMinPercent > 0 ? SellRecommendationConfig.LotMinPercent : 20;

            decimal profitMaxPercent = SellRecommendationConfig.ProfitMaxPercent > 0 ? SellRecommendationConfig.ProfitMaxPercent * 0.01m + 1 : 1.8m;
            decimal profitMidPercent = SellRecommendationConfig.ProfitMidPercent > 0 ? SellRecommendationConfig.ProfitMidPercent * 0.01m + 1 : 1.5m;
            decimal profitMinPercent = SellRecommendationConfig.ProfitMinPercent > 0 ? SellRecommendationConfig.ProfitMinPercent * 0.01m + 1 : 1.2m;

            if (args is null)
                return false;

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
                        .Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Buy)
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
                if ((lotMax + lotMid + lotMin) < stockCount)
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
                result.LotMin = (int)lotMin;
                result.LotMid = (int)lotMid;
                result.LotMax = (int)lotMax;
                result.PriceMin = priceMin;
                result.PriceMid = priceMid;
                result.PriceMax = priceMax;

                //Вычтем с низа рекомендаций проданые лоты по порядку
                foreach (var operation in operations.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Sell))
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

            return true;

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
        static List<SellRecommendation> GetRecommendations(string userId, IQueryable<Ticker> tickers, Rating[] ratings, StockTransaction[] stockTransactions)
        {
            var result = new List<SellRecommendation>();

            var transactionGroups = stockTransactions.Join(tickers, x => x.TickerId, y => y.Id, (x, y) =>
            new
            {
                y.CompanyId,
                Transaction = x,
                LotValue = y.Lot.Value
            }).GroupBy(x => x.CompanyId);

            try
            {
                foreach (var item in transactionGroups)
                {
                    var lotValues = item.Select(x => x.LotValue).ToArray();

                    for (int i = 1; i < lotValues.Length; i++)
                        if (lotValues[i - 1] != lotValues[i])
                            continue;

                    var recommendationArgs = new SellRecommendationArgs();
                    var companyTransactions = item.Select(x => x.Transaction);

                    recommendationArgs.Lot = lotValues.First();
                    recommendationArgs.BuyValue = companyTransactions.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Buy).Sum(x => x.Quantity);
                    recommendationArgs.SellValue = companyTransactions.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Sell).Sum(x => x.Quantity);

                    if (recommendationArgs.BuyValue > recommendationArgs.SellValue)
                    {
                        recommendationArgs.StockTransactions = companyTransactions;
                        recommendationArgs.RatingPlace = ratings.FirstOrDefault(x => x.CompanyId == item.Key)?.Place ?? 0;
                        recommendationArgs.RatingCount = ratings.Length;

                        var recommendation = new SellRecommendation { UserId = userId, CompanyId = item.Key };
                        if (CalculateSellRecommendation(recommendationArgs, recommendation))
                            result.Add(recommendation);
                    }
                }

                return result;
            }
            catch
            {
                return result;
            }
        }
        static SellRecommendation GetRecommendation(string userId, long companyId, int lot, Rating[] ratings, StockTransaction[] companyStockTransactions)
        {
            try
            {
                var recommendationArgs = new SellRecommendationArgs
                {
                    Lot = lot,
                    BuyValue = companyStockTransactions.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Buy).Sum(x => x.Quantity),
                    SellValue = companyStockTransactions.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Sell).Sum(x => x.Quantity)
                };

                if (recommendationArgs.BuyValue > recommendationArgs.SellValue)
                {
                    recommendationArgs.StockTransactions = companyStockTransactions;
                    recommendationArgs.RatingPlace = ratings.FirstOrDefault(x => x.CompanyId == companyId)?.Place ?? 0;
                    recommendationArgs.RatingCount = ratings.Length;

                    var recommendation = new SellRecommendation { CompanyId = companyId, UserId = userId };

                    return CalculateSellRecommendation(recommendationArgs, recommendation) ? recommendation : null;
                }
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #endregion

        public async Task<bool> ResetCalculatorDataAsync(DataBaseType dbType, string[] userIds) =>
                await SetCoeffitientsAsync(dbType)
                && await SetRatingAsync(dbType)
                && await SetBuyRecommendationsAsync(dbType)
                && await SetSellRecommendationsAsync(dbType, userIds);
    }
}
