using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Relationship.InterfaceForeignKey
{
    public interface IExchangeFK
    {
        long ExchangeId { get; set; }
        Exchange Exchange { get; set; }
    }
}
