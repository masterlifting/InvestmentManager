namespace InvestmentManager.Models
{
    public class StockTransactionModel : BaseBrokerReport
    {
        public bool IsHave { get; set; }
        public long Identifier { get; set; }
        public decimal Cost { get; set; }
        public int Quantity { get; set; }
        public long TickerId { get; set; }
        public long StatusId { get; set; }
        public long ExchangeId { get; set; }
    }
}
