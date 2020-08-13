using InvestmentManager.Entities.Calculate;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Calculator
{
    public interface IInvestCalculator
    {
        Task<List<Coefficient>> GetComplitedCoeffitientsAsync();
        Task<List<Rating>> GetCompleatedRatingsAsync();
        List<BuyRecommendation> GetCompleatedBuyRecommendations(IEnumerable<Rating> ratings);
        List<SellRecommendation> GetCompleatedSellRecommendations(IQueryable<IdentityUser> users, IEnumerable<Rating> ratings);
    }
}
