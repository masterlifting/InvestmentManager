namespace InvestmentManager.Models.EntityModels
{
    public class CoefficientModel
    {
        public int Year { get; set; }
        public int Quarter { get; set; }

        public decimal PE { get; set; }
        public decimal PB { get; set; }
        public decimal DebtLoad { get; set; }
        public decimal Profitability { get; set; }
        public decimal ROA { get; set; }
        public decimal ROE { get; set; }
        public decimal EPS { get; set; }
    }
}
