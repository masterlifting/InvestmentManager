using System.Threading.Tasks;

namespace InvestmentManager.Services.Interfaces
{
    public interface IPriceService
    {
        /// <summary>
        /// Update prices of companies
        /// </summary>
        /// <param name="period">Period in days</param>
        /// <returns>Count company of updated prices</returns>
        Task<int> DownloadNewStockPricesAsync(int period);
        /// <summary>
        /// Set weekend in stocks
        /// </summary>
        /// <returns></returns>
        Task SetWeekendAsync();
    }
}
