using InvestmentManager.Entities.Calculate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.Calculator
{
    public interface IInvestCalculator
    {
        Task<List<Coefficient>> GetComplitedCoeffitientsAsync();
        Task<List<Rating>> GetCompleatedRatingsAsync();
        List<BuyRecommendation> GetCompleatedBuyRecommendations(IEnumerable<Rating> ratings);
        List<SellRecommendation> GetCompleatedSellRecommendations(IEnumerable<string> userIds, IEnumerable<Rating> ratings);
    }
}
