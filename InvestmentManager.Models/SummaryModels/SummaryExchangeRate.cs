using System;

namespace InvestmentManager.Models.SummaryModels
{
    public class SummaryExchangeRate
    {
        public DateTime DateLastOperation { get; set; }
        public string StatusName { get; set; }
        public decimal Rate { get; set; }
    }
}
