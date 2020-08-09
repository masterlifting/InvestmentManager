using InvestmentManager.Entities.Market;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.StockPriceFinder.Interfaces
{
    public interface IPriceService
    {
        Task<List<Price>> GetPriceListAsync(string providerName, long tickerId, string ticker, string providerUri);
        List<Price> NewPriceFilter(IEnumerable<Price> prices, long tickerId);
    }
}
