using System;

namespace InvestmentManager.ClientModels.CompanyModels
{
    public class ClientCompanyTransactionsSummary
    {
        public DateTime Date { get; set; }
        public decimal Cost { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public int ActualLot { get; set; }
        public decimal CurrentProfit { get; set; }
    }
}
