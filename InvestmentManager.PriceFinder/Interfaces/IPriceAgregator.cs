using InvestmentManager.Entities.Market;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.StockPriceFinder.Interfaces
{
    public interface IPriceAgregator
    {
        Task<List<Price>> FindNewPriciesAsync(long tickerId, string ticker, string providerUri);
    }
}
