namespace InvestmentManager.Web.Models.CommonModels
{
    public class StartPageComponentModel
    {
        public string CompanyName { get; set; }
        public string SectorName { get; set; }
        public string IndustryName { get; set; }

        public decimal LastPrice { get; set; }
        public decimal BuyPrice { get; set; }

        public int LastYearReport { get; set; }
        public int LastQuarterReport { get; set; }

        public string ReportSource { get; set; }
        public int Place { get; set; }

        public long CompanyId { get; set; }
        public long SectorId { get; set; }
        public long IndustryId { get; set; }
    }
}
