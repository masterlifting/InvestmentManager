using InvestManager.Entities.Market;

namespace InvestManager.Entities.Relationship.InterfaceForeignKey
{
    public interface IIsinFK
    {
        long IsinId { get; set; }
        Isin Isin { get; set; }
    }
}
