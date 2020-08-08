namespace InvestmentManager.Web.Models.FinancialModels
{
    public class ReportBodyModel
    {
        public int Year { get; set; }
        public int Quarter { get; set; }

        public long StockVolume { get; set; }
        public decimal Dividends { get; set; }

        public decimal Obligations { get; set; }
        public decimal LongTermDebt { get; set; }

        public decimal Revenue { get; set; }
        public decimal NetProfit { get; set; }

        public decimal GrossProfit { get; set; }
        public decimal CashFlow { get; set; }

        public decimal Assets { get; set; }
        public decimal Turnover { get; set; }
        public decimal ShareCapital { get; set; }
    }
}
