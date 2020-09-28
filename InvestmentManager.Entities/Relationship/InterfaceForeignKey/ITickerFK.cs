using InvestManager.Entities.Market;

namespace InvestManager.Entities.Relationship.InterfaceForeignKey
{
    public interface ITickerFK
    {
        public long TickerId { get; set; }
        public Ticker Ticker { get; set; }
    }
}
