namespace InvestmentManager.ClientModels.CompanyModels
{
    public class ClientCompanySummary
    {
        public string Sector { get; set; }
        public string Industry { get; set; }
        public string Currency { get; set; }
        public int ActualLot { get; set; }
        public decimal CurrentProfit { get; set; }
        public decimal DividendSum { get; set; }
        public string RatingPlace { get; set; }
    }
}
