namespace InvestmentManager.ViewModels.CompanyModels
{
    public class CompanyViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = DefaultData.loading;
    }
}
