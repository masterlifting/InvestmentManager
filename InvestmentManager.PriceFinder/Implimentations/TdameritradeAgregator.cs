using InvestmentManager.Entities.Market;
using InvestmentManager.StockPriceFinder.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InvestmentManager.StockPriceFinder.Implimentations
{
    public class TdameritradeAgregator : IPriceAgregator
    {
        private static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("ru-RU");
        private readonly IServiceProvider serviceProvider;
        const string apiKey = "X9GOW9DSLWI9IAAF5U8UF5Z1UFITUWFS";
        public TdameritradeAgregator(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

        public async Task<List<Price>> FindNewPriciesAsync(long tickerId, string ticker, string providerUri)
        {
            var priceList = new List<Price>();
            string historyPriceQuery = @$"{providerUri}/{ticker}/pricehistory?apikey={apiKey}&periodType=year&period=1&frequencyType=daily&frequency=1&needExtendedHoursData=false";
            string currentPriceQuery = @$"{providerUri}/{ticker}/quotes?apikey={apiKey}";

            var tdameritradeClient = serviceProvider.GetService<CustomHttpClient>();
            #region History price
            var historyPriceResponse = await tdameritradeClient.GetPriceAsync(historyPriceQuery);
            var historyPricies = await historyPriceResponse.Content.ReadFromJsonAsync<TdameritradeJsonModel>().ConfigureAwait(false);

            foreach (var price in historyPricies.Candles.ToList())
            {
                priceList.Add(new Price
                {
                    TickerId = tickerId,
                    DateUpdate = DateTime.Now,
                    BidDate = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddMilliseconds(double.Parse(price.Datetime.ToString(), culture)),
                    Value = Convert.ToDecimal(price.High),
                    CurrencyId = 1
                });
            }
            #endregion
            #region Current price
            var currentPriceResponse = await tdameritradeClient.GetPriceAsync(currentPriceQuery);
            string currentPrice = await currentPriceResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (currentPrice != null)
            {
                var unixTimeString = Regex.Match(currentPrice, "\"regularMarketTradeTimeInLong\":(.*?),\"").Groups[1].Value;
                var unix = double.TryParse(unixTimeString, NumberStyles.Number, CultureInfo.CurrentCulture, out double unixTime);

                if (!unix) return priceList;

                DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(unixTime);

                if (priceList.Count > 0 && date.Date == priceList.Last().BidDate.Date) return priceList;

                var lastString = Regex.Match(currentPrice, "\"lastPrice\":(.*?),\"").Groups[1].Value;
                decimal.TryParse(lastString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal lastPrice);

                if (lastPrice != default)
                {
                    priceList.Add(new Price
                    {
                        TickerId = tickerId,
                        DateUpdate = DateTime.Now,
                        BidDate = date,
                        Value = lastPrice,
                        CurrencyId = 1
                    });
                }

            }
            #endregion
            return priceList;
        }
    }
    public class TdameritradeJsonModel
    {
        public IEnumerable<Candle> Candles { get; set; }
    }
    public class Candle
    {
        public double High { get; set; }
        public object Datetime { get; set; }
    }
}
