namespace InvestManager.ViewModels.PortfolioModels
{
    public class PortfolioFinancialInfoModel
    {
        public string Summary { get; set; } = DefaultData.loading;
        public string SummaryR { get; set; } = DefaultData.loading;
        public string CleanInvestSum { get; set; } = DefaultData.loading;
        public string CirculationSum { get; set; } = DefaultData.loading;
        public string FreeSum { get; set; } = DefaultData.loading;
        public string Comissions { get; set; } = DefaultData.loading;
        public string Dividends { get; set; } = DefaultData.loading;
        public string AvgDollarBuy { get; set; } = DefaultData.loading;
        public string AvgDollarSell { get; set; } = DefaultData.loading;
        public string PercentProfit { get; set; } = DefaultData.loading;
    }
}
