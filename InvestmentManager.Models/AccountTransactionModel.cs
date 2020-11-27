namespace InvestmentManager.Models
{
    public class AccountTransactionModel : BaseBrokerReport
    {
        public long StatusId { get; set; }
        public decimal Amount { get; set; }
    }
}
