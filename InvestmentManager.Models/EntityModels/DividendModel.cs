namespace InvestmentManager.Models.EntityModels
{
    public class DividendModel : BaseBrokerReport
    {
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public long IsinId { get; set; }
    }
}
