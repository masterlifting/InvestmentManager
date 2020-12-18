using System;

namespace InvestmentManager.Models.SummaryModels
{
    public class SummaryComission
    {
        public DateTime DateFirstComission { get; set; }
        public DateTime DateLastComission { get; set; }
        public decimal Amount { get; set; }
    }
}
