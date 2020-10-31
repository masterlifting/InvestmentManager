using InvestmentManager.ViewModels.ErrorModels;

namespace InvestmentManager.ViewModels.PriceModels
{
    public class PriceShortModel
    {
        public string DateUpdate { get; set; }
        public string LastPrice { get; set; }
        public ErrorBaseModel Error { get; set; } = new ErrorBaseModel();
    }
}
