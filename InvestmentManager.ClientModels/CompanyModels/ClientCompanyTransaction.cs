namespace InvestmentManager.ClientModels.CompanyModels
{
    public class ClientCompanyTransaction : BrokerReportBase
    {
        public long Identifier { get; set; }
        public decimal Cost { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public long TickerId { get; set; }
        public long StatusId { get; set; }
        public long ExchangeId { get; set; }
    }
}
