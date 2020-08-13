using InvestmentManager.Calculator.ConfigurationBinding;
using InvestmentManager.Calculator.Implimentations;
using InvestmentManager.Calculator.Models;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Calculate;
using InvestmentManager.Entities.Market;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Identity;
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

        async Task<Rating> CalculateRatingAsync(CalculatedArgs calculatedArgs, long companyId)
        {
            if (calculatedArgs is null || companyId == 0)
                throw new NullReferenceException("Недостаточно данных для расчета рейтинга");

            var currentRating = calculatedArgs.CurrentRating;
            var newRating = new Rating();

            if (calculatedArgs.Prices != null && calculatedArgs.Prices.Any())
                newRating.PriceComparisonValue = await Task.Run(() => new PriceCalculate(calculatedArgs.Prices).GetPricieComporision()).ConfigureAwait(false);

            if (calculatedArgs.Reports != null && calculatedArgs.Reports.Any())
            {
                var reportCalculate = new ReportCalculate(calculatedArgs.Reports);
                newRating.ReportComparisonValue = await Task.Run(() => reportCalculate.GetReportComporision()).ConfigureAwait(false);
                newRating.CashFlowPositiveBalanceValue = await Task.Run(() => reportCalculate.GetCashFlowBalance()).ConfigureAwait(false);
            }

            if (calculatedArgs.Coefficients != null && calculatedArgs.Coefficients.Any())
            {
                var coefficientCalculate = new CoefficientCalculate(calculatedArgs.Coefficients);
                newRating.CoefficientComparisonValue = await Task.Run(() => coefficientCalculate.GetCoefficientComparison()).ConfigureAwait(false);
                newRating.CoefficientAverageValue = await Task.Run(() => coefficientCalculate.GetCoefficientAverage()).ConfigureAwait(false);
            }

            return RecalculateRating(newRating, currentRating, companyId);

            static Rating RecalculateRating(Rating newRating, Rating currentRating, long companyId)
            {
                var ratingResult = new Rating();

                if (newRating != null)
                {
                    if (currentRating != null)
                        ratingResult = currentRating;
                    else
                        ratingResult.CompanyId = companyId;

                    // Перепись значений
                    if (newRating.CoefficientComparisonValue != default)
                        ratingResult.CoefficientComparisonValue = newRating.CoefficientComparisonValue;

                    if (newRating.CoefficientAverageValue != default)
                        ratingResult.CoefficientAverageValue = newRating.CoefficientAverageValue;

                    if (newRating.PriceComparisonValue != default)
                        ratingResult.PriceComparisonValue = newRating.PriceComparisonValue;

                    if (newRating.ReportComparisonValue != default)
                        ratingResult.ReportComparisonValue = newRating.ReportComparisonValue;

                    if (newRating.CashFlowPositiveBalanceValue != default)
                        ratingResult.CashFlowPositiveBalanceValue = newRating.CashFlowPositiveBalanceValue;

                    var newResultList = new List<decimal>
                {
                    ratingResult.CoefficientComparisonValue
                    , ratingResult.CoefficientAverageValue
                    , ratingResult.PriceComparisonValue
                    , ratingResult.ReportComparisonValue
                    , ratingResult.CashFlowPositiveBalanceValue
                };
                    // Вычисление результата
                    var tempValue = newResultList.Where(x => x != default);
                    ratingResult.Result = tempValue.Any() ? tempValue.Average() : 0;

                    ratingResult.DateUpdate = DateTime.Now;
                    ratingResult.Place = default;
                }

                return ratingResult;
            }
        }
        Coefficient CalculateReportCoefficient(Report report, decimal price, long coefficientId = 0)
        {
            return report != null
                 ? new Coefficient() { Id = coefficientId, ReportId = report.Id, PE = Pe(), PB = PB(), DebtLoad = DebtLoad(), Profitability = Profitability(), ROA = Roa(), ROE = Roe(), EPS = Eps() }
                 : throw new NullReferenceException("Нет отчета для расчета бизнес коэффициентов.");

            decimal Eps() => report.StockVolume != 0 ? ((report.NetProfit * 1_000_000) / report.StockVolume) : 0;
            decimal Profitability() => report.Revenue == 0 || report.Assets == 0 ? 0 : ((report.NetProfit / report.Revenue) + (report.Revenue / report.Assets)) / 2;
            decimal Roa() => report.Assets != 0 ? (report.NetProfit / report.Assets) * 100 : 0;
            decimal Roe() => report.ShareCapital != 0 ? (report.NetProfit / report.ShareCapital) * 100 : 0;
            decimal DebtLoad() => report.Assets != 0 ? report.Obligations / report.Assets : 0;
            decimal Pe() => Eps() != 0 && price > 0 ? price / Eps() : 0;
            decimal PB() => (report.Assets - report.Obligations) != 0 && price > 0 && report.StockVolume > 0 ? (price * report.StockVolume) / ((report.Assets - report.Obligations) * 1_000_000) : 0;
        }
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

            var operations = args.StockTransactions.OrderBy(x => x.DateOperation.Date).Select(x => new { x.TransactionStatusId, x.Quantity, x.Cost });

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
                result.CompanyId = args.CompanyId;
                result.LotMin = (int)lotMin;
                result.LotMid = (int)lotMid;
                result.LotMax = (int)lotMax;
                result.PriceMin = priceMin;
                result.PriceMid = priceMid;
                result.PriceMax = priceMax;

                //Теперь вычтем с низа рекомендаций проданые лоты по порядку
                foreach (var operation in operations.Where(x => x.TransactionStatusId == 4))
                {
                    sellCount = operation.Quantity / args.Lot;

                    if (result.LotMid > 0 && sellCount > 0)
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

        public async Task<List<Coefficient>> GetComplitedCoeffitientsAsync()
        {
            var result = new List<Coefficient>();
            var companyIds = unitOfWork.Company.GetAll().Select(x => x.Id).ToList();
            foreach (long companyId in companyIds)
            {
                var data = await unitOfWork.Coefficient.GetSortedCoefficientCalculatingDataAsync(companyId).ConfigureAwait(false);

                foreach (var (price, report) in data)
                {
                    result.Add(CalculateReportCoefficient(report, price));
                }
            }
            return result;
        }
        public async Task<List<Rating>> GetCompleatedRatingsAsync()
        {
            var result = new List<Rating>();
            var companyIds = unitOfWork.Company.GetAll().Select(x => x.Id).ToList();
            foreach (long companyId in companyIds)
            {
                var calculatorArgs = new CalculatedArgs
                {
                    CurrentRating = unitOfWork.Rating.GetAll().FirstOrDefault(x => x.CompanyId == companyId),
                    Coefficients = await unitOfWork.Coefficient.GetSortedCoefficientsAsync(companyId).ConfigureAwait(false),
                    Prices = await unitOfWork.Price.GetCustomPricesAsync(companyId, 24, OrderType.OrderBy).ConfigureAwait(false),
                    Reports = unitOfWork.Report.GetAll().Where(x => x.CompanyId == companyId && x.IsChecked == true).OrderBy(x => x.DateReport.Date)
                };

                result.Add(await CalculateRatingAsync(calculatorArgs, companyId).ConfigureAwait(false));
            }

            var comparer = new CustomCompare<Rating>();
            result.Sort(comparer);

            for (int i = 0; i < result.Count; i++)
            {
                result[i].Place = i + 1;
            }

            return result;
        }
        public List<SellRecommendation> GetCompleatedSellRecommendations(IQueryable<IdentityUser> users, IEnumerable<Rating> ratings)
        {
            var result = new List<SellRecommendation>();
            var accounts = unitOfWork.Account.GetAll();
            var stockTransactions = unitOfWork.StockTransaction.GetAll();
            var tickers = unitOfWork.Ticker.GetTickerIncludedLot();
            var companies = unitOfWork.Company.GetAll();
            int ratingCount = ratings.Count();

            foreach (var user in users.ToList())
            {
                var userStockTransactions = stockTransactions.Join(accounts.Where(x => x.UserId.Equals(user.Id)), x => x.AccountId, y => y.Id, (x, y) => x);
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
                        sellRecommendation.UserId = user.Id;
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
            var prices = unitOfWork.Price.GetGroupedPrices(12, OrderType.OrderBy);
            
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
    }
    class CustomCompare<T> : IComparer<T> where T : Rating
    {
        public int Compare(T x, T y) => x.Result < y.Result ? 1 : x.Result > y.Result ? -1 : 0;
    }
}
