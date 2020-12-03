namespace InvestmentManager.Models.EntityModels
{
    public class StockTransactionModel : BaseBrokerReport
    {
        public long Identifier { get; set; }
        public decimal Cost { get; set; }
        public int Quantity { get; set; }
        public long TickerId { get; set; }
        public long StatusId { get; set; }
        public string StatusName { get; set; }
        public long ExchangeId { get; set; }
    }
}
