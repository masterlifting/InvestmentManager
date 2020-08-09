using System.Collections.Generic;

namespace InvestmentManager.Web.Models.FinancialModels
{
    public class ReportModel
    {
        public long? CompanyId { get; set; }
        public IEnumerable<ReportHeadModel> Headers { get; set; }
        public IEnumerable<ReportBodyModel> ReportBodyModels { get; }
    }
    public class ReportHeadModel
    {
        public long CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int LastYear { get; set; }
        public int LastQuarter { get; set; }
        public int TotalCount { get; set; }
    }
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
