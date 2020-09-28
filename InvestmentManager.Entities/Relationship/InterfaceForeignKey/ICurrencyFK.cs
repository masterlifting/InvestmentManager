using InvestManager.Entities.Market;

namespace InvestManager.Entities.Relationship.InterfaceForeignKey
{
    public interface ICurrencyFK
    {
        long CurrencyId { get; set; }
        Currency Currency { get; set; }
    }
}
