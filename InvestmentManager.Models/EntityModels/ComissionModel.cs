namespace InvestmentManager.Models.EntityModels
{
    public class ComissionModel : BaseBrokerReport
    {
        public long TypeId { get; set; }
        public string TypeName { get; set; }
        public decimal Amount { get; set; }
    }
}
