using System;
using System.Collections.Generic;

namespace InvestmentManager.Models.SummaryModels
{
    public class SummaryExchangeRate
    {
        public DateTime DateLastOperation { get; set; }
        public string StatusName { get; set; }
        public decimal Rate { get; set; }

        public List<SummaryExchangeRateDetail> Details { get; set; } = new List<SummaryExchangeRateDetail>();
    }
    public class SummaryExchangeRateDetail
    {
        public string Currency { get; set; }
        public decimal AvgPurchasedRate { get; set; }
        public decimal AvgSoldRate { get; set; }
    }
}
