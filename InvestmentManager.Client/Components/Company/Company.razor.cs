using InvestmentManager.Client.Services.HttpService;
using InvestmentManager.ViewModels.CompanyModels;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Components.Company
{
    public partial class Company : ComponentBase
    {
        [Inject]
        private CustomHttpClient HttpClient { get; set; }

        public List<CompanyViewModel> Companies = new List<CompanyViewModel>();
        protected override async Task OnInitializedAsync()
        {
            Companies = await HttpClient.GetResultAsync<List<CompanyViewModel>>("companies").ConfigureAwait(false);
        }
    }
}
