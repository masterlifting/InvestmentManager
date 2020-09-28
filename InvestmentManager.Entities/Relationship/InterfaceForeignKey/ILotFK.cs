using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Relationship.InterfaceForeignKey
{
    public interface ILotFK
    {
        long LotId { get; set; }
        Lot Lot { get; set; }
    }
}
