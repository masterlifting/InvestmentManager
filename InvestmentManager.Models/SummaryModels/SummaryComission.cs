using System;
using System.Collections.Generic;

namespace InvestmentManager.Models.SummaryModels
{
    public class SummaryComission
    {
        public DateTime DateFirstComission { get; set; }
        public DateTime DateLastComission { get; set; }
        public List<SummaryComissionDetail> Details { get; set; } = new List<SummaryComissionDetail>();
    }
    public class SummaryComissionDetail
    {
        public string Currency { get; set; }
        public decimal Amount { get; set; }
    }
}
