using System;

namespace InvestmentManager.Web.Models.PortfolioModels
{
    public class RateOperationModel
    {
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public DateTime DateOperation { get; set; }
        public string TypeOperation { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
    }
}
