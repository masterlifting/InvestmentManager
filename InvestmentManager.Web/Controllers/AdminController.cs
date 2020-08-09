using InvestmentManager.Calculator;
using InvestmentManager.Repository;
using InvestmentManager.Web.Models.AdminModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Web.Controllers
{
    [Authorize(Roles = "pestunov")]
    public class AdminController : Controller
    {
        const string localUrl = "~/admin";
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IInvestmentCalculator calculator;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;

        public AdminController(
            IUnitOfWorkFactory unitOfWork
            , IInvestmentCalculator calculator
            , UserManager<IdentityUser> userManager
            , IWebHostEnvironment webHostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.calculator = calculator;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
        }
        const string solutionName = nameof(InvestmentManager);

        public IActionResult Index()
        {
            var model = new AdminLoadModel();

            if (Process.GetProcessesByName($"{solutionName}.{model.PriceFinderProcessName}").Any())
                model.StockPriceFinderInProcess = true;

            if (Process.GetProcessesByName($"{solutionName}.{model.ReportFinderProcessName}").Any())
                model.ReportFinderInProcess = true;

            return View(model);
        }

        public IActionResult StartBackgroundProcess(string processName)
        {
            if (!string.IsNullOrWhiteSpace(processName))
            {
                string folderName = string.Intern("Debug");
                if (webHostEnvironment.IsProduction())
                    folderName = string.Intern("Release");

                var startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = @$"D:\IT\{solutionName}\{solutionName}.{processName}\bin\{folderName}\netcoreapp3.1\{solutionName}.{processName}.exe",
                    WindowStyle = ProcessWindowStyle.Normal,
                };
                Task.Run(() => { Process.Start(startInfo); });
            }

            return LocalRedirect($"{localUrl}/{nameof(Index)}");
        }

        public IActionResult CancelOperation(string processName)
        {
            foreach (var item in Process.GetProcessesByName($"{solutionName}.{processName}"))
            {
                item.Kill();
                item.Dispose();
            }

            return LocalRedirect($"{localUrl}/{nameof(Index)}");
        }

        public async Task<IActionResult> UpdateAllCalculatorResult()
        {
            //pre delete all
            unitOfWork.Rating.TruncateAndReseed();
            unitOfWork.Coefficient.TruncateAndReseed();
            unitOfWork.BuyRecommendation.TruncateAndReseed();
            unitOfWork.SellRecommendation.TruncateAndReseed();
            await unitOfWork.CompleteAsync().ConfigureAwait(false);

            // add new
            unitOfWork.Coefficient.CreateEntities(await calculator.GetComplitedCoeffitientsAsync().ConfigureAwait(false));
            await unitOfWork.CompleteAsync().ConfigureAwait(false);

            var ratings = await calculator.GetCompleatedRatingsAsync().ConfigureAwait(false);
            unitOfWork.Rating.CreateEntities(ratings);
            unitOfWork.SellRecommendation.CreateEntities(calculator.GetCompleatedSellRecommendations(userManager.Users, ratings));
            unitOfWork.BuyRecommendation.CreateEntities(calculator.GetCompleatedBuyRecommendations(ratings));

            await unitOfWork.CompleteAsync().ConfigureAwait(false);

            return LocalRedirect($"~/Home/{nameof(Index)}");
        }
    }
}
