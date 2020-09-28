using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Relationship.InterfaceForeignKey
{
    public interface ICurrencyFK
    {
        long CurrencyId { get; set; }
        Currency Currency { get; set; }
    }
}
