using System;

namespace InvestmentManager.Models.SummaryModels
{
    public class SummaryStockTransaction
    {
        public bool IsHave { get; set; } = false;
        public DateTime DateTransaction { get; set; }
        public decimal Cost { get; set; }
        public int Quantity { get; set; }
        public string StatusName { get; set; }
    }
}
