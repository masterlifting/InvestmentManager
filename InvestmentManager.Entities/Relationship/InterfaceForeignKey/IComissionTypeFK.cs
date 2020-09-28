using InvestManager.Entities.Broker;

namespace InvestManager.Entities.Relationship.InterfaceForeignKey
{
    public interface IComissionTypeFK
    {
        long ComissionTypeId { get; set; }
        ComissionType ComissionType { get; set; }
    }
}
