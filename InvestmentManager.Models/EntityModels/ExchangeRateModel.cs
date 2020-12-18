namespace InvestmentManager.Models.EntityModels
{
    public class ExchangeRateModel : BaseBrokerReport
    {
        public long Identifier { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public long StatusId { get; set; }
        public string StatusName { get; set; }
    }
}
