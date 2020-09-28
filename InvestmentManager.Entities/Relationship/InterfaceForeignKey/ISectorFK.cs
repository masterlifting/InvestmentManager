using InvestManager.Entities.Market;

namespace InvestManager.Entities.Relationship.InterfaceForeignKey
{
    public interface ISectorFK
    {
        long SectorId { get; set; }
        Sector Sector { get; set; }
    }
}
