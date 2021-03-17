using InvestmentManager.Entities.Market;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

namespace InvestmentManager.Services.Implimentations
{
    public class PriceService : IPriceService
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IWebService webService;

        public PriceService(
            IUnitOfWorkFactory unitOfWork
            , IWebService webService)
        {
            this.unitOfWork = unitOfWork;
            this.webService = webService;
        }
        public async Task<int> DownloadNewStockPricesAsync(int period)
        {
            var dateNow = DateTime.Now.Date;

            if (dateNow.DayOfWeek == DayOfWeek.Sunday || dateNow.DayOfWeek == DayOfWeek.Saturday)
                return 0;

            #region Set weekend

            var dateStart = dateNow.AddDays(-15).Date;

            var targetWeekend = unitOfWork.Weekend.GetAll().Where(x => x.ExchangeWeekend.Date >= dateStart && x.ExchangeWeekend.Date <= dateNow)
                .ToArray()
                .OrderByDescending(x => x.ExchangeWeekend);

            #endregion
            #region Data preparation
            var startPriceDate = DateTime.Now.AddDays(-period);

            var exchanges = unitOfWork.Exchange.GetAll();
            var tickers = await unitOfWork.Price.GetTickersByPricesAsync();
            var prices = (await unitOfWork.Price.GetAll().Where(x => x.BidDate >= startPriceDate).ToArrayAsync())
                .GroupBy(x => x.TickerId);

            var tickerIds = tickers.Select(x => x.Id);
            var tickerIdsWhithPrices = prices.Select(x => x.Key);
            var tickerIdsWithOutPrices = tickerIds.Except(tickerIdsWhithPrices);

            var dataWithPricesGrouped = prices.Select(x => new { TickerId = x.Key, DateLastPrice = x.OrderBy(x => x.BidDate).Last().BidDate, IdLastPrice = x.OrderBy(x => x.BidDate).Last().Id })
                .Join(tickers, x => x.TickerId, y => y.Id, (x, y) => new { x.DateLastPrice, x.IdLastPrice, Ticker = y, y.ExchangeId })
                .Join(exchanges, x => x.ExchangeId, y => y.Id, (x, y) => new { x.Ticker, Exchange = y, x.DateLastPrice, x.IdLastPrice })
                .GroupBy(x => x.Exchange);

            var dataWithOutPricesGrouped = tickers
                .Where(x => tickerIdsWithOutPrices.Contains(x.Id))
                .Join(exchanges, x => x.ExchangeId, y => y.Id, (x, y) => new { Ticker = x, Exchange = y })
                .GroupBy(x => x.Exchange);

            List<PricePreparatedData> resultPreparatedData = new();

            if (dataWithPricesGrouped is not null)
                foreach (var item in dataWithPricesGrouped)
                {
                    var exchangeWeekend = targetWeekend.Where(x => x.ExchangeId == item.Key.Id);
                    if (!exchangeWeekend.Select(x => x.ExchangeWeekend.Date).Contains(dateNow))
                        resultPreparatedData.Add(new PricePreparatedData(item.Key, item.Select(x => (x.Ticker, x.DateLastPrice, x.IdLastPrice)), exchangeWeekend));
                }

            if (dataWithOutPricesGrouped is not null)
                foreach (var item in dataWithOutPricesGrouped)
                {
                    var exchangeWeekend = targetWeekend.Where(x => x.ExchangeId == item.Key.Id);
                    if (!exchangeWeekend.Select(x => x.ExchangeWeekend.Date).Contains(dateNow))
                    {
                        List<(Ticker, DateTime, long)> priceData = new();
                        foreach (var data in item)
                            priceData.Add((data.Ticker, default, default));

                        resultPreparatedData.Add(new PricePreparatedData(item.Key, priceData, exchangeWeekend));
                    }
                }
            #endregion
            #region Data agregation
            var result = new List<Price>();
            var agregator = new PriceAgregator(webService);
            for (int i = 0; i < resultPreparatedData.Count; i++)
            {
                var separatedData = new PriceSeparatedData(resultPreparatedData[i]);
                if (separatedData.CurrentModel.PriceData.Any())
                    result.AddRange(await agregator.GetCurrentPricesAsync(separatedData.CurrentModel));
                if (separatedData.HistoryModel.PriceData.Any())
                    result.AddRange(await agregator.GetHistoryPricesAsync(separatedData.HistoryModel));
            }
            #endregion
            #region Data save
            if (result.Any())
            {
                var newPrices = result.Where(x => x.Id == default);
                var currentPrices = result.Where(x => x.Id != default).ToArray();
                int companyCountWithPrices = result.GroupBy(x => x.TickerId).Count();

                if (newPrices.Any())
                    await unitOfWork.Price.CreateEntitiesAsync(newPrices);
                if (currentPrices.Any())
                {
                    for (int i = 0; i < currentPrices.Length; i++)
                    {
                        var price = await unitOfWork.Price.FindByIdAsync(currentPrices[i].Id);
                        
                        price.Value = currentPrices[i].Value;
                        price.BidDate = currentPrices[i].BidDate;
                        price.DateUpdate = currentPrices[i].DateUpdate;
                    }
                }

                return await unitOfWork.CompleteAsync() ? companyCountWithPrices : -1;
            }
            else
                return 0;
            #endregion
        }
        public async Task SetWeekendAsync()
        {
            var mmvbWeekend = new List<DateTime>
            {
                new DateTime(2021,01,01),
                new DateTime(2021,01,04),
                new DateTime(2021,01,05),
                new DateTime(2021,01,06),
                new DateTime(2021,01,07),
                new DateTime(2021,01,08),
                new DateTime(2021,02,22),
                new DateTime(2021,02,23),
                new DateTime(2021,05,03),
                new DateTime(2021,05,10),
                new DateTime(2021,06,14),
                new DateTime(2021,11,04),
                new DateTime(2021,11,05),
                new DateTime(2021,12,31)
            };
            var spbWeekend = new List<DateTime>
            {
                new DateTime(2021,01,01),
                new DateTime(2021,01,18),
                new DateTime(2021,02,15),
                new DateTime(2021,05,31),
                new DateTime(2021,06,05),
                new DateTime(2021,09,06),
                new DateTime(2021,10,11),
                new DateTime(2021,11,11),
                new DateTime(2021,11,25),
                new DateTime(2021,12,24),
                new DateTime(2021,12,31)
            };

            unitOfWork.Weekend.DeleteAndReseedPostgres();

            await unitOfWork.Weekend.CreateEntitiesAsync(mmvbWeekend.Select(x => new Weekend
            {
                ExchangeId = (long)ExchangeTypes.mmvb,
                ExchangeWeekend = x
            }));
            await unitOfWork.Weekend.CreateEntitiesAsync(spbWeekend.Select(x => new Weekend
            {
                ExchangeId = (long)ExchangeTypes.spb,
                ExchangeWeekend = x
            }));

            await unitOfWork.CompleteAsync();
        }
    }
    abstract class BasePriceData
    {
        public BasePriceData(Exchange exchange) => Exchange = exchange;
        public Exchange Exchange { get; }
    }

    class PricePreparatedData : BasePriceData
    {
        public Weekend[] Weekend { get; }
        public (Ticker Ticker, DateTime PriceDate, long PriceId)[] PriceData { get; }

        public PricePreparatedData(Exchange exchange, IEnumerable<(Ticker, DateTime, long)> priceData, IEnumerable<Weekend> weekend) : base(exchange)
        {
            Weekend = weekend is not null ? weekend.ToArray() : Array.Empty<Weekend>();
            PriceData = priceData is not null ? priceData.ToArray() : Array.Empty<(Ticker, DateTime, long)>();
        }
    }

    class PriceSeparatedData
    {
        private readonly DateTime[] weekend;

        public PriceCurrentModel CurrentModel { get; private set; }
        public PriceHistoryModel HistoryModel { get; private set; }

        public PriceSeparatedData(PricePreparatedData preparatedData)
        {
            CurrentModel = new PriceCurrentModel(preparatedData.Exchange);
            HistoryModel = new PriceHistoryModel(preparatedData.Exchange);
            weekend = preparatedData.Weekend.Select(x => x.ExchangeWeekend).OrderByDescending(x => x.Date).ToArray();
            SeparateData(preparatedData.PriceData);
        }

        void SeparateData(IEnumerable<(Ticker Ticker, DateTime DateLastPrice, long PriceId)> data)
        {
            if (data is not null)
            {
                (Ticker Ticker, DateTime PriceDate, long PriceId)[] priceData = data.ToArray();

                for (int i = 0; i < priceData.Length; i++)
                {
                    if (priceData[i].PriceDate == default)
                    {
                        HistoryModel.PriceData.Add((priceData[i].Ticker, priceData[i].PriceDate));
                        continue;
                    }

                    var lastPriceDate = priceData[i].PriceDate.Date;

                    if (lastPriceDate == DateTime.Now.Date)
                        CurrentModel.PriceData.Add((priceData[i].Ticker, priceData[i].PriceDate, priceData[i].PriceId));
                    else if (lastPriceDate == DateTime.Now.AddDays(-1).Date)
                        CurrentModel.PriceData.Add((priceData[i].Ticker, priceData[i].PriceDate, default));
                    else if (DateTime.Now.DayOfWeek == DayOfWeek.Monday && lastPriceDate.DayOfWeek == DayOfWeek.Friday)
                        CurrentModel.PriceData.Add((priceData[i].Ticker, priceData[i].PriceDate, default));
                    else if (weekend.Contains(priceData[i].PriceDate.AddDays(-1).Date))
                    {
                        var previousDate = priceData[i].PriceDate.AddDays(-1).Date;
                        var currentWeekend = weekend.Where(x => x.Date <= previousDate).ToArray();
                        var controlDate = currentWeekend[0].Date;

                        for (int j = 0; j < currentWeekend.Length; j++)
                        {
                            var checkDate = controlDate.AddDays(-j).Date;

                            if (checkDate != currentWeekend[j] && checkDate == lastPriceDate)
                                CurrentModel.PriceData.Add((priceData[i].Ticker, priceData[i].PriceDate, default));
                            else
                                break;
                        }
                    }
                    else
                        HistoryModel.PriceData.Add((priceData[i].Ticker, priceData[i].PriceDate));
                }
            }
        }
    }
    class PriceCurrentModel : BasePriceData
    {
        public PriceCurrentModel(Exchange exchange) : base(exchange) { }
        public List<(Ticker Ticker, DateTime priceDate, long PriceId)> PriceData { get; set; } = new();
    }
    class PriceHistoryModel : BasePriceData
    {
        public PriceHistoryModel(Exchange exchange) : base(exchange) { }
        public List<(Ticker Ticker, DateTime PriceDate)> PriceData { get; set; } = new();
    }

    class PriceAgregator
    {
        private readonly Dictionary<string, IPriceAgregator> agregator;
        public PriceAgregator(IWebService webService)
        {
            agregator = new Dictionary<string, IPriceAgregator>
            {
                {"ММВБ", new MoexAgregator(webService)},
                { "СПБ", new TdameritradeAgregator(webService) }
            };
        }

        public async Task<List<Price>> GetCurrentPricesAsync(PriceCurrentModel model) =>
            model?.Exchange is not null && agregator.ContainsKey(model.Exchange.Name)
                ? await agregator[model.Exchange.Name].GetCurrentPricesAsync(model.PriceData)
                : new List<Price>();
        public async Task<List<Price>> GetHistoryPricesAsync(PriceHistoryModel model) =>
            model?.Exchange is not null && agregator.ContainsKey(model.Exchange.Name)
                ? await agregator[model.Exchange.Name].GetHistoryPricesAsync(model.PriceData)
                : new List<Price>();
    }
    interface IPriceAgregator
    {
        Task<List<Price>> GetCurrentPricesAsync(IEnumerable<(Ticker ticker, DateTime priceDate, long priceId)> data);
        Task<List<Price>> GetHistoryPricesAsync(IEnumerable<(Ticker ticker, DateTime priceDate)> data);
    }

    class MoexAgregator : IPriceAgregator
    {
        #region Описание API Московской биржи
        /*
            * вся актуальная информация о российских компаниях одним запросом
            * https://iss.moex.com/iss/engines/stock/markets/shares/boards/TQBR/securities.json
            * вся актуальная информация о иностранных компаниях одним запросом в рублях
            * https://iss.moex.com/iss/engines/stock/markets/foreignshares/boards/FQBR/securities.json

            * актуальная информация по конкретному инструменту российского рынка
            * https://iss.moex.com/iss/engines/stock/markets/shares/boards/TQBR/securities/{ticker}.json
            * актуальная информация по конкретному инструменту иностранного рынка в рублях
            * https://iss.moex.com/iss/engines/stock/markets/foreignshares/boards/FQBR/securities/{ticker}-RM.json

            * история по конкретному инструменту российской компании за период
            * https://iss.moex.com/iss/history/engines/stock/markets/shares/securities/{ticker}/candles.json?from={year}-{month}-{day}&interval=24&start=0
            * история по конкретному инструменту иностранной компании за период в рублях
            * https://iss.moex.com/iss/history/engines/stock/markets/foreignshares/securities/{ticker}-RM/candles.json?from={year}-{month}-{day}&interval=24&start=0
         */
        #endregion
        const string currentPricesUrl = @"https://iss.moex.com/iss/engines/stock/markets/shares/boards/TQBR/securities.json";
        static string HistoryPriceUrlBuilder(string ticker, DateTime date) => @$"https://iss.moex.com/iss/history/engines/stock/markets/shares/securities/{ticker}/candles.json?from={date.Year}-{date.Month}-{date.Day}&interval=24&start=0";

        private readonly IWebService webService;
        public MoexAgregator(IWebService webService) => this.webService = webService;

        public async Task<List<Price>> GetCurrentPricesAsync(IEnumerable<(Ticker ticker, DateTime priceDate, long priceId)> data)
        {
            List<Price> result = new();

            var response = await webService.GetDataAsync(currentPricesUrl);

            if (!response.IsSuccessStatusCode)
                return result;

            var responseData = await response.Content.ReadFromJsonAsync<CurrentPricesModel>();

            List<IntermediateModel> intermediateModels = new();

            for (int i = 0; i < responseData.Marketdata.Data.Length; i++)
            {
                string ticker = responseData.Marketdata.Data[i][0]?.ToString();
                string price = responseData.Marketdata.Data[i][12]?.ToString();
                string dataTime = responseData.Marketdata.Data[i][48]?.ToString();

                if (!string.IsNullOrWhiteSpace(price) && !string.IsNullOrWhiteSpace(ticker))
                    intermediateModels.Add(new IntermediateModel(ticker, price, dataTime));
            }

            foreach (var i in intermediateModels.Join(data, x => x.TickerName, y => y.ticker.Name, (x, y) => new
            {
                PriceId = y.priceId,
                TickerId = y.ticker.Id,
                LastDatePrice = y.priceDate,

                NewDatePrice = x.DataTime,
                NewPrice = x.Price
            }))
            {
                if (!DateTime.TryParse(i.NewDatePrice, out DateTime newBidDate))
                    continue;

                if (i.PriceId == default && newBidDate.AddHours(-1).Date == i.LastDatePrice.Date)
                    continue;

                if (!decimal.TryParse(i.NewPrice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal price))
                    continue;

                result.Add(new Price
                {
                    Id = i.PriceId,
                    TickerId = i.TickerId,
                    CurrencyId = (long)CurrencyTypes.usd,
                    DateUpdate = DateTime.Now,
                    BidDate = newBidDate,
                    Value = price
                });
            }

            return result;
        }
        public async Task<List<Price>> GetHistoryPricesAsync(IEnumerable<(Ticker ticker, DateTime priceDate)> data)
        {
            List<Price> result = new();

            foreach (var item in data)
            {
                DateTime priceDate = item.priceDate != default ? item.priceDate.AddDays(1) : DateTime.Now.AddYears(-1);

                var response = await webService.GetDataAsync(HistoryPriceUrlBuilder(item.ticker.Name, priceDate));
                if (!response.IsSuccessStatusCode)
                    continue;

                var responseData = await response.Content.ReadFromJsonAsync<HistoryPriceModel>();

                List<IntermediateModel> intermediateModels = new();
                for (int i = 0; i < responseData.History.Data.Length; i++)
                {
                    string price = responseData.History.Data[i][8]?.ToString();
                    string dataTime = responseData.History.Data[i][1]?.ToString();

                    if (!string.IsNullOrWhiteSpace(price) && !string.IsNullOrWhiteSpace(dataTime))
                        intermediateModels.Add(new IntermediateModel("", price, dataTime));
                }

                for (int i = 0; i < intermediateModels.Count; i++)
                {
                    if (!DateTime.TryParse(intermediateModels[i].DataTime, out DateTime newBidDate))
                        continue;

                    if (!decimal.TryParse(intermediateModels[i].Price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal price))
                        continue;

                    result.Add(new Price
                    {
                        TickerId = item.ticker.Id,
                        CurrencyId = (long)CurrencyTypes.rub,
                        DateUpdate = DateTime.Now,
                        BidDate = newBidDate,
                        Value = price
                    });
                }
            }

            return result;
        }

        #region Current model by Json
        public class CurrentPricesModel
        {
            public Securities Securities { get; set; }
            public Marketdata Marketdata { get; set; }
        }
        public class Securities
        {
            public string[] Columns { get; set; }
            public object[][] Data { get; set; }
        }
        public class Marketdata
        {
            public string[] Columns { get; set; }
            public object[][] Data { get; set; }
        }
        #endregion
        #region History model by Json
        public class HistoryPriceModel
        {
            public History History { get; set; }
        }
        public class History
        {
            public string[] Columns { get; set; }
            public object[][] Data { get; set; }
        }
        #endregion
        class IntermediateModel
        {
            public IntermediateModel(string tickerName, string price, string dataTime)
            {
                TickerName = tickerName;
                Price = price;
                DataTime = dataTime;
            }
            internal string TickerName { get; }
            internal string Price { get; }
            public string DataTime { get; }
        }
    }
    class TdameritradeAgregator : IPriceAgregator
    {
        #region Описание API иностранной биржи
        /*
       * актуальная информация по нескольким тикерам иностранных компаний (на время торгов по времени сша)
       * https://api.tdameritrade.com/v1/marketdata/quotes?apikey=X9GOW9DSLWI9IAAF5U8UF5Z1UFITUWFS&symbol={ticker1}%2C{ticker2}
       * https://api.tdameritrade.com/v1/marketdata/{ticker}/pricehistory?apikey=X9GOW9DSLWI9IAAF5U8UF5Z1UFITUWFS&periodType=year&period=1&frequencyType=daily&frequency=1&needExtendedHoursData=false"
       * 
      */
        #endregion
        static string HistoryPriceUrlBuilder(string ticker) => @$"https://api.tdameritrade.com/v1/marketdata/{ticker}/pricehistory?apikey=X9GOW9DSLWI9IAAF5U8UF5Z1UFITUWFS&periodType=year&period=1&frequencyType=daily&frequency=1&needExtendedHoursData=false";

        private readonly IWebService webService;
        public TdameritradeAgregator(IWebService webService) => this.webService = webService;

        public async Task<List<Price>> GetCurrentPricesAsync(IEnumerable<(Ticker ticker, DateTime priceDate, long priceId)> data)
        {
            List<Price> result = new();
            string currentPricesUrl = @"https://api.tdameritrade.com/v1/marketdata/quotes?apikey=X9GOW9DSLWI9IAAF5U8UF5Z1UFITUWFS&symbol=";

            foreach (var ticker in data.Select(x => x.ticker.Name))
                currentPricesUrl += @$"{ticker}%2C";

            currentPricesUrl = currentPricesUrl.Remove(currentPricesUrl.Length - 3, 3);

            Dictionary<string, Dictionary<string, object>> parsedResult;
            try
            {
                var response = await webService.GetDataAsync(currentPricesUrl);

                if (!response.IsSuccessStatusCode)
                    return result;

                string responseString = await response.Content.ReadAsStringAsync();
                parsedResult = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(responseString);
            }
            catch
            {
                return result;
            }

            foreach (var i in parsedResult.Join(data, x => x.Key.ToLowerInvariant(), y => y.ticker.Name.ToLowerInvariant(), (x, y) => new
            {
                PriceId = y.priceId,
                TickerId = y.ticker.Id,
                LastDatePrice = y.priceDate,

                NewDatePrice = x.Value["regularMarketTradeTimeInLong"].ToString(),
                NewPrice = x.Value["lastPrice"].ToString()
            }))
            {
                if (!double.TryParse(i.NewDatePrice, out double milliseconds))
                    continue;

                DateTime newBidDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(milliseconds);
                if (i.PriceId == default && newBidDate.Date == i.LastDatePrice.Date)
                    continue;

                if (!decimal.TryParse(i.NewPrice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal price))
                    continue;

                result.Add(new Price
                {
                    Id = i.PriceId,
                    TickerId = i.TickerId,
                    CurrencyId = (long)CurrencyTypes.usd,
                    DateUpdate = DateTime.Now,
                    BidDate = newBidDate,
                    Value = price
                });
            }

            return result;
        }
        public async Task<List<Price>> GetHistoryPricesAsync(IEnumerable<(Ticker ticker, DateTime priceDate)> data)
        {
            List<Price> result = new();

            foreach (var item in data)
            {
                if (item.priceDate.Date == DateTime.Now.AddDays(-2).Date)
                {
                    var loadTime = item.priceDate.AddHours(56);
                    if (loadTime > DateTime.Now)
                        continue;
                }

                var ticker = item.ticker.Name.ToUpperInvariant();
                var response = await webService.GetDataAsync(HistoryPriceUrlBuilder(ticker));
                if (!response.IsSuccessStatusCode)
                    continue;

                var responseData = await response.Content.ReadAsStringAsync();
                var deserialazedData = JsonSerializer.Deserialize<CandleList>(responseData);

                List<Price> intermediatePrices = new();
                for (int i = 0; i < deserialazedData.candles.Length; i++)
                {
                    decimal price = deserialazedData.candles[i].high;
                    ulong milliseconds = deserialazedData.candles[i].datetime;

                    DateTime newBidDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(milliseconds);

                    intermediatePrices.Add(new Price
                    {
                        TickerId = item.ticker.Id,
                        CurrencyId = (long)CurrencyTypes.usd,
                        DateUpdate = DateTime.Now,
                        BidDate = newBidDate,
                        Value = price
                    });
                }

                result.AddRange(intermediatePrices.Where(x => x.BidDate > item.priceDate));
            }

            return result;
        }

        #region History model by xml
        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [Serializable()]
        [System.ComponentModel.DesignerCategory("code")]
        [System.Xml.Serialization.XmlType(AnonymousType = true)]
        [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
        public partial class CandleList
        {

            private CandleListCandles[] candlesField;

            private string symbolField;

            private bool emptyField;

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItem("candles", IsNullable = false)]
            public CandleListCandles[] candles
            {
                get
                {
                    return this.candlesField;
                }
                set
                {
                    this.candlesField = value;
                }
            }

            /// <remarks/>
            public string symbol
            {
                get
                {
                    return this.symbolField;
                }
                set
                {
                    this.symbolField = value;
                }
            }

            /// <remarks/>
            public bool empty
            {
                get
                {
                    return this.emptyField;
                }
                set
                {
                    this.emptyField = value;
                }
            }
        }
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class CandleListCandles
        {

            private decimal openField;

            private decimal highField;

            private decimal lowField;

            private decimal closeField;

            private uint volumeField;

            private ulong datetimeField;

            /// <remarks/>
            public decimal open
            {
                get
                {
                    return this.openField;
                }
                set
                {
                    this.openField = value;
                }
            }

            /// <remarks/>
            public decimal high
            {
                get
                {
                    return this.highField;
                }
                set
                {
                    this.highField = value;
                }
            }

            /// <remarks/>
            public decimal low
            {
                get
                {
                    return this.lowField;
                }
                set
                {
                    this.lowField = value;
                }
            }

            /// <remarks/>
            public decimal close
            {
                get
                {
                    return this.closeField;
                }
                set
                {
                    this.closeField = value;
                }
            }

            /// <remarks/>
            public uint volume
            {
                get
                {
                    return this.volumeField;
                }
                set
                {
                    this.volumeField = value;
                }
            }

            /// <remarks/>
            public ulong datetime
            {
                get
                {
                    return this.datetimeField;
                }
                set
                {
                    this.datetimeField = value;
                }
            }
        }
        #endregion
    }
}
