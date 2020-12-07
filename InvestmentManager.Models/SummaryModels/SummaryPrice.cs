using System;

namespace InvestmentManager.Models.SummaryModels
{
    public class SummaryPrice
    {
        public DateTime DateUpdate { get; set; }
        public DateTime DatePrice { get; set; }
        public decimal Cost { get; set; }
    }
}
