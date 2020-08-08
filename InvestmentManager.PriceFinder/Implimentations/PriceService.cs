using InvestmentManager.Entities.Market;
using InvestmentManager.Repository;
using InvestmentManager.StockPriceFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.StockPriceFinder.Implimentations
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

            return foundPrice.Any() ? await NewPriceFilter(foundPrice, tickerId).ConfigureAwait(false) : resultPricies;
        }
        public async Task<List<Price>> NewPriceFilter(IEnumerable<Price> pricies, long tickerId)
        {
            var filteredList = new List<Price>();
            var dateFromPriceLists = pricies.Select(x => x.BidDate.Date);
            var dateFromPriceListsCount = dateFromPriceLists.Count();
            var dateFromDb = await unitOfWork.Price.GetSortedPriceDateAsync(tickerId, dateFromPriceListsCount).ConfigureAwait(false);

            var newDate = dateFromPriceLists.Except(dateFromDb);

            if (newDate.Any())
            {
                foreach (var date in newDate)
                {
                    var newPrice = pricies.First(x => x.BidDate.Date == date.Date);

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
