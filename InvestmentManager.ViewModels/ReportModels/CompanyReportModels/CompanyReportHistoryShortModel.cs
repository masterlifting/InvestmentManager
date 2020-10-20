namespace InvestmentManager.ViewModels.ReportModels.CompanyReportModels
{
    public class CompanyReportHistoryShortModel
    {
        public string ReportCount { get; set; } = DefaultData.loading;
        public string DateUpdate { get; set; } = DefaultData.loading;
        public string DateLastReport { get; set; } = DefaultData.loading;
        public string LastQuarter { get; set; } = DefaultData.loading;
        public string LastYear { get; set; } = DefaultData.loading;
    }
}
