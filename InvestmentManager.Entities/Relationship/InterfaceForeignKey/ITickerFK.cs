using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Relationship.InterfaceForeignKey
{
    public interface ITickerFK
    {
        public long TickerId { get; set; }
        public Ticker Ticker { get; set; }
    }
}
