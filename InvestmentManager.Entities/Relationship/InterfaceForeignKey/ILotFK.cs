using InvestManager.Entities.Market;

namespace InvestManager.Entities.Relationship.InterfaceForeignKey
{
    public interface ILotFK
    {
        long LotId { get; set; }
        Lot Lot { get; set; }
    }
}
