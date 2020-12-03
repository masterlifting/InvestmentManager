using System;

namespace InvestmentManager.Models.SummaryModels
{
    public class SummaryDividend
    {
        public bool IsHave { get; set; } = false;
        public DateTime DateLastDividend { get; set; }
        public decimal LastAmount { get; set; }
        public decimal TotalSum { get; set; }
    }
}
