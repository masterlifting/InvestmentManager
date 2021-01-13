using InvestmentManager.Entities.Market;
using InvestmentManager.Repository;
using InvestmentManager.PriceFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentManager.Services.Interfaces;

namespace InvestmentManager.PriceFinder.Implimentations
{
    public class PriceService : IPriceService
    {
        private readonly Dictionary<string, IPriceAgregator> priceProviders;
        private readonly IUnitOfWorkFactory unitOfWork;

        public PriceService(IWebService httpService, IUnitOfWorkFactory unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            // !!! Если появятся новые источники цен, то просто добавь их сюда и реализуй IPriceAgregator
            priceProviders = new Dictionary<string, IPriceAgregator>
            {
                { "moex", new MoexAgregator(httpService) },
                { "tdameritrade", new TdameritradeAgregator(httpService) }
            };
        }

        public async Task<List<Price>> GetPriceListAsync(string providerName, long tickerId, string ticker, string providerUri)
        {
            var resultPricies = new List<Price>();

            if (!priceProviders.ContainsKey(providerName))
                return resultPricies;

            var foundPrice = await priceProviders[providerName].FindNewPriciesAsync(tickerId, ticker, providerUri).ConfigureAwait(false);

            return foundPrice.Any() ? await NewPriceFilterAsync(foundPrice, tickerId).ConfigureAwait(false) : resultPricies;
        }

        async Task<List<Price>> NewPriceFilterAsync(IEnumerable<Price> pricies, long tickerId)
        {
            var filteredList = new List<Price>();
            var dateFromPriceLists = pricies.Select(x => x.BidDate.Date);
            var dateFromPriceListsCount = dateFromPriceLists.Count();
            var dateFromDb = (await unitOfWork.Price.GetLastDatesAsync(tickerId, dateFromPriceListsCount).ConfigureAwait(false)).Select(x => x.Date);

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
}
