using InvestmentManager.ViewModels.ResultModels;

namespace InvestmentManager.ViewModels.PriceModels
{
    public class PriceShortModel
    {
        public string DateUpdate { get; set; }
        public string LastPrice { get; set; }
        public ResultBaseModel Error { get; set; } = new ResultBaseModel();
    }
}
