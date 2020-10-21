using InvestmentManager.Calculator;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize(Roles = "pestunov")]
    public class AdminController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IInvestCalculator calculator;
        private readonly UserManager<IdentityUser> userManager;

        public AdminController(
            IUnitOfWorkFactory unitOfWork
            , IInvestCalculator calculator
            , UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.calculator = calculator;
            this.userManager = userManager;
        }


        [HttpGet("recalculateall")]
        public async Task<IActionResult> RecalculateAll()
        {
            try
            {
                //pre delete all sql
                /*/
                unitOfWork.Rating.TruncateAndReseedSQL();
                unitOfWork.Coefficient.TruncateAndReseedSQL();
                unitOfWork.BuyRecommendation.TruncateAndReseedSQL();
                unitOfWork.SellRecommendation.TruncateAndReseedSQL();
                /*/
                //pre delete all Postgres
                unitOfWork.Rating.DeleteAndReseedPostgres();
                unitOfWork.Coefficient.DeleteAndReseedPostgres();
                unitOfWork.BuyRecommendation.DeleteAndReseedPostgres();
                unitOfWork.SellRecommendation.DeleteAndReseedPostgres();
                //*/
                await unitOfWork.CompleteAsync().ConfigureAwait(false);

                // add new
                await unitOfWork.Coefficient.CreateEntitiesAsync(await calculator.GetComplitedCoeffitientsAsync().ConfigureAwait(false)).ConfigureAwait(false);
                await unitOfWork.CompleteAsync().ConfigureAwait(false);

                var ratings = await calculator.GetCompleatedRatingsAsync().ConfigureAwait(false);
                await unitOfWork.Rating.CreateEntitiesAsync(ratings).ConfigureAwait(false);
                var sellRecommendations = calculator.GetCompleatedSellRecommendations(userManager.Users, ratings);
                await unitOfWork.SellRecommendation.CreateEntitiesAsync(sellRecommendations).ConfigureAwait(false);
                await unitOfWork.BuyRecommendation.CreateEntitiesAsync(calculator.GetCompleatedBuyRecommendations(ratings)).ConfigureAwait(false);

                await unitOfWork.CompleteAsync().ConfigureAwait(false);

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}


