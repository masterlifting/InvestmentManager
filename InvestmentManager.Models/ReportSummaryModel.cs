using System;

namespace InvestmentManager.Models
{
    public class ReportSummaryModel
    {
        public bool IsHave { get; set; }
        public DateTime DateUpdate { get; set; }
        public DateTime DateLastReport { get; set; }
        public int LastReportYear { get; set; }
        public int LastReportQuarter { get; set; }
        public int ReportsCount { get; set; }
    }
}
