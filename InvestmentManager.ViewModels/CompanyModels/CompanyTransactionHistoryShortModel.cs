using InvestmentManager.ViewModels.ErrorModels;

namespace InvestmentManager.ViewModels.CompanyModels
{
    public class CompanyTransactionHistoryShortModel
    {
        public string LastDateTransaction { get; set; }
        public string Status { get; set; }
        public string Lot { get; set; }
        public string Price { get; set; }
        public ErrorBaseModel Error { get; set; } = new ErrorBaseModel();
    }
}
