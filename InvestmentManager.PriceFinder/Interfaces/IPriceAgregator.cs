using InvestManager.Entities.Market;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestManager.PriceFinder.Interfaces
{
    internal interface IPriceAgregator
    {
        Task<List<Price>> FindNewPriciesAsync(long tickerId, string ticker, string providerUri);
    }
}
