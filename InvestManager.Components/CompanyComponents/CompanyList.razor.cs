using InvestManager.ViewModels.CompanyModels;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace InvestManager.Client.Components.Company
{
    public partial class CompanyList : ComponentBase
    {
        [Parameter]
        public List<CompanyViewModel> Companies { get; set; }
    }
}
