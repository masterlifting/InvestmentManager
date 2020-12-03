namespace InvestmentManager.Models.EntityModels
{
    public class AccountTransactionModel : BaseBrokerReport
    {
        public long StatusId { get; set; }
        public string StatusName { get; set; }
        public decimal Amount { get; set; }
    }
}
