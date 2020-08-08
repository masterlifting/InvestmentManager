using InvestmentManager.Entities.Broker;

namespace InvestmentManager.Entities.Relationship.InterfaceForeignKey
{
    public interface IAccountFK
    {
        long AccountId { get; set; }
        Account Account { get; set; }
    }
}
