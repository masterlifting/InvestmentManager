using InvestManager.ViewModels.CompanyModels;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace InvestManager.Client.Components.Company
{
    public partial class Company : ComponentBase
    {
        [Inject]
        private HttpClient HttpClient { get; set; }

        public List<CompanyViewModel> Companies = new List<CompanyViewModel>();
        protected override async Task OnInitializedAsync()
        {
            Companies = await HttpClient.GetFromJsonAsync<List<CompanyViewModel>>("companies");
        }
    }
}
