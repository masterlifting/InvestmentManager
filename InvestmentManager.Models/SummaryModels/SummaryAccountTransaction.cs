using System;

namespace InvestmentManager.Models.SummaryModels
{
    public class SummaryAccountTransaction
    {
        public bool IsHave { get; set; } = false;
        public DateTime DateLastTransaction { get; set; }
        public string StatusName { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalAddedSum { get; set; }
    }
}
