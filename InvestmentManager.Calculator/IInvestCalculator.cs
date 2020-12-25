using InvestmentManager.Entities.Market;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

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

        Task<bool> SetBuyRecommendationsAsync(DataBaseType dbType);
        Task<bool> SetSellRecommendationsAsync(DataBaseType dbType, string[] userIds);
        Task<bool> SetSellRecommendationsAsync(string userId);

        Task<bool> ResetCalculatingDataAsync(DataBaseType dbType, string userId);
        Task<bool> ResetCalculatingDataAsync(DataBaseType dbType, string[] userIds);
    }
}
