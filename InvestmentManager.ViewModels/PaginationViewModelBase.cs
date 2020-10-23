using System.Collections.Generic;

namespace InvestmentManager.ViewModels
{
    public class PaginationViewModelBase
    {
        public List<ViewModelBase> Items { get; set; }
        public Pagination Pagination { get; set; }
    }
}
