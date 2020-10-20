namespace InvestmentManager.ViewModels.CompanyModels
{
    public class CompanyAdditionalInfoShortModel
    {
        public string Sector { get; set; } = DefaultData.loading;
        public string Industry { get; set; } = DefaultData.loading;
        public string Currency { get; set; } = DefaultData.loading;
    }
}
