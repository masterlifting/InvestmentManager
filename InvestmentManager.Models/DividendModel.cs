namespace InvestmentManager.Models
{
    public class DividendModel : BaseBrokerReport
    {
        public bool IsHave { get; set; }
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public long IsinId { get; set; }
    }
}
