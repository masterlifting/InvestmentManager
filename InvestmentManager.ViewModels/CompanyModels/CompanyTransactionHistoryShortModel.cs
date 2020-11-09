using InvestmentManager.ViewModels.ResultModels;

namespace InvestmentManager.ViewModels.CompanyModels
{
    public class CompanyTransactionHistoryShortModel
    {
        public string LastDateTransaction { get; set; }
        public string Status { get; set; }
        public string Lot { get; set; }
        public string Price { get; set; }
        public ResultBaseModel Error { get; set; } = new ResultBaseModel();
    }
}
