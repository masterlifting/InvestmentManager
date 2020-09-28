using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Relationship.InterfaceForeignKey
{
    public interface ISectorFK
    {
        long SectorId { get; set; }
        Sector Sector { get; set; }
    }
}
