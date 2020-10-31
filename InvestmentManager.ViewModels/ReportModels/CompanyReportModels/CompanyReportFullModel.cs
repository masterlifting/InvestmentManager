using System;

namespace InvestmentManager.ViewModels.ReportModels.CompanyReportModels
{
    public class CompanyReportFullModel
    {
        public DateTime DateReport { get; set; }
        public int Quarter { get; set; }
        public decimal Revenue { get; set; }
        public decimal NetProfit { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal CashFlow { get; set; }
        public decimal Assets { get; set; }
        public decimal Turnover { get; set; }
        public decimal ShareCapital { get; set; }
        public decimal Dividend { get; set; }
        public decimal Obligation { get; set; }
        public decimal LongTermDebt { get; set; }
    }
}
