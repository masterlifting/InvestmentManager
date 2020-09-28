using InvestManager.Entities.Broker;

namespace InvestManager.Entities.Relationship.InterfaceForeignKey
{
    public interface IAccountFK
    {
        long AccountId { get; set; }
        Account Account { get; set; }
    }
}
