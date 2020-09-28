using InvestManager.BrokerService;
using InvestManager.BrokerService.Models;
using InvestManager.Mapper.Interfaces;
using InvestManager.Repository;
using InvestManager.ViewModels;
using InvestManager.ViewModels.PortfolioModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace InvestManager.Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IInvestBrokerService brokerService;
        private readonly IPortfolioMapper portfolioMapper;
        private readonly IMemoryCache memoryCache;
        public const string defaultResult = DefaultData.errorData;

        public PortfolioController(
            IUnitOfWorkFactory unitOfWork
            , UserManager<IdentityUser> userManager
            , IInvestBrokerService brokerService
            , IPortfolioMapper portfolioMapper
            , IMemoryCache memoryCache)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.brokerService = brokerService;
            this.portfolioMapper = portfolioMapper;
            this.memoryCache = memoryCache;
        }

        [Route("financial")]
        [HttpGet]
        public async Task<PortfolioFinancialInfoModel> Get(string dollar)
        {
            var result = new PortfolioFinancialInfoModel();

            #region Загрузка данных
            string userId = userManager.GetUserId(User);
            bool isRate = decimal.TryParse(dollar, out decimal rate);
            if (!isRate)
                rate = 0;
            var accountIds = await unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userId)).Select(x => x.Id).ToArrayAsync().ConfigureAwait(false);
            var companyIds = await unitOfWork.Company.GetAll().Select(x => x.Id).ToArrayAsync().ConfigureAwait(false);
            var dividends = unitOfWork.Dividend.GetAll().Where(x => accountIds.Contains(x.AccountId));
            var comissions = unitOfWork.Comission.GetAll().Where(x => accountIds.Contains(x.AccountId));
            var accountTransactions = unitOfWork.AccountTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId));
            var stockTransactions = (await unitOfWork.Ticker.GetTikersIncludeTransactions(accountIds).ToArrayAsync().ConfigureAwait(false)).GroupBy(x => x.CompanyId);
            var exchangeRates = unitOfWork.ExchangeRate.GetAll().Where(x => accountIds.Contains(x.AccountId));
            var lastPrices = unitOfWork.Price.GetLastPrices(30);
            #endregion

            #region Что нужно расчитать...
            decimal cleanInvestSumRub = 0; // Чистая сумма инвестиций (рублевая часть)
            decimal cleanInvestSumUsd = 0;// Чистая сумма инвестиций (валютная часть)
            decimal circulationActualSumRub = 0; // Стоимость всех акций, которые сейчас не проданы, с учетом последней доступной цены (рублевая часть)
            decimal circulationActualSumUsd = 0;// Стоимость всех акций, которые сейчас не проданы, с учетом последней доступной цены (валютная часть)
            decimal circulationOriginalSumRub = 0; // Стоимость акций, которые сейчас не проданы, на момент их покупки (рублевая часть)
            decimal circulationOriginalSumUsd = 0;// Стоимость акций, которые сейчас не проданы, на момент их покупки (валютная часть)
            decimal fixedProfitRub = 0; // Зафиксированная прибыль по акциям, которых больше нет в портфеле (рублевая часть)
            decimal fixedProfitUsd = 0;// Зафиксированная прибыль по акциям, которых больше нет в портфеле (валютная часть)
            decimal freeSumRub = 0;// Свободных средств в портфеле (рублевая часть)
            decimal freeSumUsd = 0;// Свободных средств в портфеле (валютная часть)
            decimal avgDollarBuy = 0; //Средние курсы покупки  доллара по портфелю
            decimal avgDollarSell = 0;//Средние курсы продажи  доллара по портфелю
            decimal summarySumRub = 0;// Полная стоимость портфеля (рублевая часть)
            decimal summarySumUsd = 0;// Полная стоимость портфеля (валютная часть)
            decimal summaryAllRub = 0;// Полная стоимость портфеля в пересчете на рубли с учетом текущего курса доллара
            decimal percentProfitRub = 0;// Рублевая прибыльность в %
            decimal percentProfitUsd = 0;// Валютная прибыльность в %
            #endregion

            #region Подготовка дефолтного резудьтата
            string cleanInvestRub = defaultResult;
            string circulationActualRub = defaultResult;
            string cleanInvestUsd = defaultResult;
            string circulationActualUsd = defaultResult;
            string dividendRub = defaultResult;
            string dividendUsd = defaultResult;
            string comission = defaultResult;
            string freeRub = defaultResult;
            string freeUsd = defaultResult;
            string dollarBuy = defaultResult;
            string dollarSell = defaultResult;
            string summaryRub = defaultResult;
            string summaryUsd = defaultResult;
            string summary = defaultResult;
            string percentRub = defaultResult;
            string percentUsd = defaultResult;

            #endregion

            #region Расчеты
            // - Сумма комиссий
            decimal comissionSum = 0;
            if (comissions != null)
            {
                comissionSum = await comissions.SumAsync(x => x.Amount).ConfigureAwait(false);
                comission = comissionSum.ToString("C");
            }
            // - Дивиденды
            decimal dividendSumUsd = 0;
            decimal dividendSumRub = 0;
            if (dividends != null)
            {
                dividendSumUsd = dividends.Where(x => x.CurrencyId == 1).Sum(x => x.Amount);
                dividendSumRub = dividends.Where(x => x.CurrencyId == 2).Sum(x => x.Amount);
                dividendRub = dividendSumRub.ToString("C");
                dividendUsd = dividendSumUsd.ToString("C", new CultureInfo("en-US"));
            }
            // - Операции по счетам
            decimal investSumUsd = 0;
            decimal investSumRub = 0;
            decimal withdrowSumUsd = 0;
            decimal withdrowSumRub = 0;
            if (accountTransactions != null)
            {
                var investmentSum = accountTransactions.Where(x => x.TransactionStatusId == 1);
                var withdrowSum = accountTransactions.Where(x => x.TransactionStatusId == 2);
                if (investmentSum != null)
                {
                    investSumUsd = await investmentSum.Where(x => x.CurrencyId == 1).SumAsync(x => x.Amount).ConfigureAwait(false);
                    investSumRub = await investmentSum.Where(x => x.CurrencyId == 2).SumAsync(x => x.Amount).ConfigureAwait(false);
                }
                if (withdrowSum != null)
                {
                    withdrowSumUsd = await withdrowSum.Where(x => x.CurrencyId == 1).SumAsync(x => x.Amount).ConfigureAwait(false);
                    withdrowSumRub = await withdrowSum.Where(x => x.CurrencyId == 2).SumAsync(x => x.Amount).ConfigureAwait(false);
                }
            }
            // - Обмены валют
            decimal usdSumBuy = 0;
            decimal usdSumSell = 0;
            decimal usdQuantityBuy = 0;
            decimal usdQuantitySell = 0;
            if (exchangeRates != null)
            {
                var usdBuy = exchangeRates.Where(x => x.TransactionStatusId == 3);
                var usdSell = exchangeRates.Where(x => x.TransactionStatusId == 4);
                if (usdBuy != null)
                {
                    usdSumBuy = await usdBuy.SumAsync(x => x.Quantity * x.Rate).ConfigureAwait(false);
                    usdQuantityBuy = await usdBuy.SumAsync(x => x.Quantity).ConfigureAwait(false);
                }
                if (usdSell != null)
                {
                    usdSumSell = await usdSell.SumAsync(x => x.Quantity * x.Rate).ConfigureAwait(false);
                    usdQuantitySell = await usdSell.SumAsync(x => x.Quantity).ConfigureAwait(false);
                }
            }
            // - Пересчет операций по портфелю с учетом обмена валют
            investSumRub -= usdSumBuy;
            investSumUsd += usdQuantityBuy;
            investSumUsd -= usdQuantitySell;
            investSumRub += usdSumSell;

            if (investSumRub != 0)
            {
                cleanInvestSumRub = investSumRub - withdrowSumRub;
                cleanInvestRub = cleanInvestSumRub.ToString("C");
            }
            if (investSumUsd != 0)
            {
                cleanInvestSumUsd = investSumUsd - withdrowSumUsd;
                cleanInvestUsd = cleanInvestSumUsd.ToString("C", new CultureInfo("en-US"));
            }

            // - Расчет транзакций с акциями по разном направлениям
            foreach (var i in companyIds.Join(stockTransactions, x => x, y => y.Key, (x, y) => new
            {
                CompanyId = x,
                StockTransactions = y.Select(x => x.StockTransactions).Aggregate((x, y) => x.Union(y))
            }))
            {
                var buyTransactions = i.StockTransactions.Where(x => x.TransactionStatusId == 3);
                var sellTransactions = i.StockTransactions.Where(x => x.TransactionStatusId == 4);

                decimal buyQuantity = buyTransactions.Sum(x => x.Quantity);
                decimal sellQuantity = sellTransactions.Sum(x => x.Quantity);

                decimal fixedSum = 0;
                decimal originalSum = 0;
                decimal actualSum = 0;
                // - Если проданы все акции этой компании
                if (buyQuantity == sellQuantity && buyQuantity != 0)
                {
                    decimal buySum = buyTransactions.Sum(x => x.Quantity * x.Cost);
                    decimal sellSum = sellTransactions.Sum(x => x.Quantity * x.Cost);
                    fixedSum = sellSum - buySum;

                }
                // - Если еще есть непроданные акции в портфеле
                else if (buyQuantity > sellQuantity)
                {
                    bool isLastPrice = lastPrices.TryGetValue(i.CompanyId, out decimal lastPrice);
                    if (!isLastPrice)
                        continue;

                    actualSum = (buyQuantity - sellQuantity) * lastPrice;

                    foreach (var transaction in i.StockTransactions.OrderBy(x => x.DateOperation))
                    {
                        if (transaction.TransactionStatusId == 3)
                            originalSum += (transaction.Quantity * transaction.Cost);
                        else if (transaction.TransactionStatusId == 4)
                            originalSum -= (transaction.Quantity * transaction.Cost);
                    }
                }
                // - расклад по валютам
                if (i.StockTransactions.First().CurrencyId == 1)
                {
                    fixedProfitUsd += fixedSum;
                    circulationOriginalSumUsd += originalSum;
                    circulationActualSumUsd += actualSum;
                }
                else
                {
                    fixedProfitRub += fixedSum;
                    circulationOriginalSumRub += originalSum;
                    circulationActualSumRub += actualSum;
                }
            }

            if (cleanInvestSumRub != 0)
            {
                freeSumRub = cleanInvestSumRub - circulationOriginalSumRub + fixedProfitRub + dividendSumRub - comissionSum;
                freeRub = freeSumRub.ToString("C");
            }
            if (cleanInvestSumUsd != 0)
            {
                freeSumUsd = cleanInvestSumUsd - circulationOriginalSumUsd + fixedProfitUsd + dividendSumUsd;
                freeUsd = freeSumUsd.ToString("C", new CultureInfo("en-US"));
            }

            if (usdQuantityBuy != 0)
            {
                avgDollarBuy = usdSumBuy / usdQuantityBuy;
                dollarBuy = avgDollarBuy.ToString("C");
                if (usdQuantitySell != 0)
                {
                    avgDollarSell = usdSumSell / usdQuantitySell;
                    dollarSell = avgDollarSell.ToString("C");
                }
            }

            summarySumRub = circulationActualSumRub + freeSumRub;
            summarySumUsd = circulationActualSumUsd + freeSumUsd;
            summaryRub = summarySumRub.ToString("C");
            summaryUsd = summarySumUsd.ToString("C", new CultureInfo("en-US"));
            if (rate != 0)
            {
                summaryAllRub = summarySumRub + summarySumUsd * rate;
                summary = summaryAllRub.ToString("C");
            }

            if (cleanInvestSumRub != 0)
            {
                percentProfitRub = Math.Round((summarySumRub / cleanInvestSumRub), 2) - 1;
                percentRub = percentProfitRub.ToString("P1");
            }
            if (cleanInvestSumUsd != 0)
            {
                percentProfitUsd = Math.Round((summarySumUsd / cleanInvestSumUsd), 2) - 1;
                percentUsd = percentProfitUsd.ToString("P1");
            }

            if (circulationActualSumRub != 0)
                circulationActualRub = circulationActualSumRub.ToString("C");
            if (circulationActualSumUsd != 0)
                circulationActualUsd = circulationActualSumUsd.ToString("C", new CultureInfo("en-US"));
            #endregion

            #region Заполнение модели
            result.CleanInvestSum = $"{cleanInvestRub}|{cleanInvestUsd}";
            result.Summary = $"{summaryRub}|{summaryUsd}";
            result.SummaryR = $"{summary}";
            result.CirculationSum = $"{circulationActualRub}|{circulationActualUsd}";
            result.Comissions = $"{comission}";
            result.Dividends = $"{dividendRub}|{dividendUsd}";
            result.FreeSum = $"{freeRub}|{freeUsd}";
            result.AvgDollarBuy = $"{dollarBuy}";
            result.AvgDollarSell = $"{dollarSell}";
            result.PercentProfit = $"{percentRub}|{percentUsd}";
            #endregion

            return result;
        }

        [Route("common")]
        [HttpGet]
        public async Task<PortfolioCommonModel> Get()
        {
            string userId = userManager.GetUserId(User);
            string companyCount = defaultResult;
            int count = await unitOfWork.SellRecommendation.GetAll().Where(x => x.UserId.Equals(userId)).Select(x => x.CompanyId).CountAsync().ConfigureAwait(false);
            companyCount = count.ToString();

            return new PortfolioCommonModel { CompanyCount = companyCount };
        }

        [Route("bcsreports")]
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var files = HttpContext.Request.Form.Files;
            string userId = userManager.GetUserId(User);
            try
            {
                var parsedReports = await brokerService.GetNewReportsAsync(files, userId).ConfigureAwait(false);
                var result = portfolioMapper.MapBcsReports(parsedReports);
                foreach (var entityReportModel in parsedReports.Reports)
                    memoryCache.Set(entityReportModel.AccountName, entityReportModel, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
                return Ok(result);
            }
            catch
            {
                return BadRequest();
            }
        }

        [Route("savereports")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string accountId)
        {
            var account = unitOfWork.Account.GetAll().FirstOrDefault(x => x.Name.Equals(accountId));
            if (account != null && memoryCache.TryGetValue(accountId, out EntityReportModel saveResult))
            {
                if (saveResult.AccountTransactions.Any())
                    await unitOfWork.AccountTransaction.CreateEntitiesAsync(saveResult.AccountTransactions).ConfigureAwait(false);

                if (saveResult.StockTransactions.Any())
                    await unitOfWork.StockTransaction.CreateEntitiesAsync(saveResult.StockTransactions).ConfigureAwait(false);

                if (saveResult.Dividends.Any())
                    await unitOfWork.Dividend.CreateEntitiesAsync(saveResult.Dividends).ConfigureAwait(false);

                if (saveResult.Comissions.Any())
                    await unitOfWork.Comission.CreateEntitiesAsync(saveResult.Comissions).ConfigureAwait(false);

                if (saveResult.ExchangeRates.Any())
                    await unitOfWork.ExchangeRate.CreateEntitiesAsync(saveResult.ExchangeRates).ConfigureAwait(false);

                try
                {
                    await unitOfWork.CompleteAsync().ConfigureAwait(false);
                    return Ok();
                }
                catch (Exception)
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }
    }
}
