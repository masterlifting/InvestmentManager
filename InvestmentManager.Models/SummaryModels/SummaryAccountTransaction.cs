using System.Collections.Generic;

namespace InvestmentManager.Models.SummaryModels
{
    public class SummaryAccountTransaction
    {
        public List<SummaryAccountTransactionDetail> Details { get; set; } = new List<SummaryAccountTransactionDetail>();
    }
    public class SummaryAccountTransactionDetail
    {
        public string Currency { get; set; }
        public decimal AddedSum { get; set; }
        public decimal WithdrawnSum { get; set; }
    }
}
