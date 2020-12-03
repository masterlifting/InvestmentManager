using System;

namespace InvestmentManager.Models.SummaryModels
{
    public class SummaryPrice
    {
        public bool IsHave { get; set; } = false;
        public DateTime DateUpdate { get; set; }
        public DateTime DatePrice { get; set; }
        public decimal Cost { get; set; }
    }
}
