﻿using InvestmentManager.Entities.Calculate;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Calculator
{
    public interface IInvestmentCalculator
    {
        Task<List<Coefficient>> GetComplitedCoeffitientsAsync();
        Task<List<Rating>> GetCompleatedRatingsAsync();
        Task<List<BuyRecommendation>> GetCompleatedBuyRecommendationsAsync(IEnumerable<Rating> ratings);
        List<SellRecommendation> GetCompleatedSellRecommendations(IQueryable<IdentityUser> users, IEnumerable<Rating> ratings);
    }
}
