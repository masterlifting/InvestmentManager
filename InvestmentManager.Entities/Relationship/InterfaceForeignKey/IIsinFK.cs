using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Relationship.InterfaceForeignKey
{
    public interface IIsinFK
    {
        long IsinId { get; set; }
        Isin Isin { get; set; }
    }
}
