using InvestmentManager.Entities.Market;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.PriceFinder.Interfaces
{
    public interface IPriceService
    {
        Task<List<Price>> GetPriceListAsync(string providerName, long tickerId, string ticker, string providerUri);
    }
}
