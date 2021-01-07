using System.Collections.Generic;

namespace InvestmentManager.Models.SummaryModels
{
    public class AccountAdditionalModel
    {
        public List<AccountAdditionalDetail> Details { get; set; } = new List<AccountAdditionalDetail>();
    }
    public class AccountAdditionalDetail
    {
        public string Currency { get; set; }
        public decimal FreeSum { get; set; }
        public decimal InvestedSum { get; set; }
        public decimal? DividendSum { get; set; }
    }
}
