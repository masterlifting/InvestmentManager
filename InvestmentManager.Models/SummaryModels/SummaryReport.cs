using System;

namespace InvestmentManager.Models.SummaryModels
{
    public class SummaryReport
    {
        public bool IsHave { get; set; } = false;
        public DateTime DateUpdate { get; set; }
        public DateTime DateLastReport { get; set; }
        public int LastReportYear { get; set; }
        public int LastReportQuarter { get; set; }
        public int ReportsCount { get; set; }
    }
}
