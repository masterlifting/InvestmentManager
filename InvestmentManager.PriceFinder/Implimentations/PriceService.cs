using InvestmentManager.Entities.Market;
using InvestmentManager.Repository;
using InvestmentManager.PriceFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace InvestmentManager.PriceFinder.Implimentations
{
    public class PriceService : IPriceService
    {
        private readonly Dictionary<string, IPriceAgregator> priceProviders;
        private readonly IUnitOfWorkFactory unitOfWork;

        public PriceService(IServiceProvider serviceProvider, IUnitOfWorkFactory unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            // !!! Если появятся новые источники цен, то просто добавь их сюда и реализуй IPriceAgregator
            priceProviders = new Dictionary<string, IPriceAgregator>
            {
                { "moex", new MoexAgregator(serviceProvider) },
                { "tdameritrade", new TdameritradeAgregator(serviceProvider) }
            };
        }

        public async Task<List<Price>> GetPriceListAsync(string providerName, long tickerId, string ticker, string providerUri)
        {
            var resultPricies = new List<Price>();

            if (!priceProviders.ContainsKey(providerName)) return resultPricies;

            var foundPrice = await priceProviders[providerName].FindNewPriciesAsync(tickerId, ticker, providerUri).ConfigureAwait(false);

            return foundPrice.Any() ? NewPriceFilter(foundPrice, tickerId) : resultPricies;
        }
        
        List<Price> NewPriceFilter(IEnumerable<Price> pricies, long tickerId)
        {
            var filteredList = new List<Price>();
            var dateFromPriceLists = pricies.Select(x => x.BidDate.Date);
            var dateFromPriceListsCount = dateFromPriceLists.Count();
            var dateFromDb = unitOfWork.Price.GetLastDates(tickerId, dateFromPriceListsCount).Select(x => x.Date);

            var newDate = dateFromPriceLists.Except(dateFromDb);

            if (newDate.Any())
            {
                foreach (var date in newDate)
                {
                    var newPrice = pricies.First(x => x.BidDate.Date == date);

                    filteredList.Add(newPrice);
                }
                return filteredList;
            }

            var lastObjFromPriceList = pricies.OrderBy(x => x.BidDate.Date).Last();

            if (lastObjFromPriceList.BidDate.Date == DateTime.Now.Date)
            {
                var updatePrice = unitOfWork.Price.GetAll().Where(x => x.TickerId == tickerId).OrderBy(x => x.BidDate.Date).LastOrDefault();
                updatePrice.Value = lastObjFromPriceList.Value;
                updatePrice.DateUpdate = DateTime.Now;

                filteredList.Add(updatePrice);

                return filteredList;
            }

            return filteredList;
        }
    }
    public class CustomHttpClient
    {
        HttpClient HttpClient { get; }
        public CustomHttpClient(HttpClient httpClient) => HttpClient = httpClient;
        // Gets the list of services.
        public async Task<HttpResponseMessage> GetPriceAsync(string query)
        {
            await Task.Delay(500).ConfigureAwait(false);

            var request = new HttpRequestMessage(HttpMethod.Get, query);
            var response = await HttpClient.SendAsync(request).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Большое колличество запросов. Ждем 10 сек");
                Console.ResetColor();

                await Task.Delay(10000).ConfigureAwait(false);
                request = new HttpRequestMessage(HttpMethod.Get, query);
                response = await HttpClient.SendAsync(request).ConfigureAwait(false);
            }

            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}
