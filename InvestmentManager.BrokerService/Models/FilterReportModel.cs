using System;

namespace InvestmentManager.BrokerService.Models
{
    public class FilterReportModel
    {
        public string AccountName { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
    }
}
