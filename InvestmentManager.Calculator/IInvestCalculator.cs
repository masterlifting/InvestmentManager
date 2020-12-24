using InvestmentManager.Entities.Calculate;
using InvestmentManager.Entities.Market;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.Calculator
{
    public interface IInvestCalculator
    {
        Task<bool> SetCoeffitientsAsync(DataBaseType dbType);
        Task<bool> SetCoeffitientAsync(Report report);

        Task<bool> SetRatingAsync(DataBaseType dbType);
        Task<bool> SetRatingByPricesAsync();
        Task<bool> SetRatingByPricesAsync(long companyId);
        Task<bool> SetRatingByReportsAsync();
        Task<bool> SetRatingByReportsAsync(long companyId);
        Task<bool> SetRatingByCoefficientsAsync();
        Task<bool> SetRatingByCoefficientsAsync(long companyId);

        List<BuyRecommendation> GetCompleatedBuyRecommendations(IEnumerable<Rating> ratings);
        List<SellRecommendation> GetCompleatedSellRecommendations(IEnumerable<string> userIds, IEnumerable<Rating> ratings);


        Task SetBuyRecommendationsAsync();
        Task SetSellRecommendationsAsync(string userId);

        Task ResetCalculatingDataAsync(string userId);
    }
}
