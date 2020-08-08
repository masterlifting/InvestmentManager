using InvestmentManager.Entities.Market;
using InvestmentManager.StockPriceFinder.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace InvestmentManager.StockPriceFinder.Implimentations
{
    class MoexAgregator : IPriceAgregator
    {
        const NumberStyles style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
        private static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("ru-RU");
        private readonly IServiceProvider serviceProvider;
        static readonly DateTime date = DateTime.Now.AddDays(-366);

        public MoexAgregator(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

        public async Task<List<Price>> FindNewPriciesAsync(long tickerId, string ticker, string providerUri)
        {
            var priiceList = new List<Price>();

            #region Описание API Московской биржи
            /*
                * https://iss.moex.com/iss/engines/stock/markets/shares
                * History — запрос цен из нужного периода времени
                * Stock — торговая система(engines) «Фондовый рынок и рынок депозитов»
                * Shares — конкретный рынок(markets) «Рынок акций»
                * TQBR — конкретный режим торгов(boards) «Т+ Акции и ДР»
                * https://iss.moex.com/iss/engines/stock/markets/shares/boards/tqbr/securities/ROSB/.json - максимум информации на сегодня
                * https://iss.moex.com/iss/history/engines/stock/markets/shares/securities/ROSB.json?from=2019-01-01&interval=24&start=0 -- история цен
             */
            #endregion

            string query = $@"{providerUri}/engines/stock/markets/shares/boards/TQBR/securities/{ticker}/candles.json?from={date.Year}-{date.Month}-{date.Day}&interval=24&start=0";

            var clientMoex = serviceProvider.GetService<CustomHttpClient>();
            var response = await clientMoex.GetPriceAsync(query);
            var priceData = await response.Content.ReadFromJsonAsync<MoexJsonModel>().ConfigureAwait(false);

            foreach (var price in priceData.Candles.Data)
            {
                bool p = decimal.TryParse(price[2].ToString(), style, CultureInfo.InvariantCulture, out decimal thisPrice);
                bool d = DateTime.TryParse(price[6].ToString(), culture, DateTimeStyles.AdjustToUniversal, out DateTime thisDate);
                if (p && d)
                    priiceList.Add(new Price
                    {
                        TickerId = tickerId,
                        DateUpdate = DateTime.Now,
                        BidDate = thisDate,
                        Value = thisPrice,
                        CurrencyId = 2
                    });
            }

            return priiceList;
        }
    }

    public class MoexJsonModel
    {
        public Candles Candles { get; set; }
    }
    public class Candles
    {
        public List<List<object>> Data { get; set; }
    }
}
