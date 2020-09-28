using InvestmentManager.Entities.Broker;

namespace InvestmentManager.Entities.Relationship.InterfaceForeignKey
{
    public interface IComissionTypeFK
    {
        long ComissionTypeId { get; set; }
        ComissionType ComissionType { get; set; }
    }
}
