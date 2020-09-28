using InvestManager.Entities.Market;

namespace InvestManager.Entities.Relationship.InterfaceForeignKey
{
    public interface IExchangeFK
    {
        long ExchangeId { get; set; }
        Exchange Exchange { get; set; }
    }
}
