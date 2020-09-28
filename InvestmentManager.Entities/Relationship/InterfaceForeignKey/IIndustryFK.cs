using InvestManager.Entities.Market;

namespace InvestManager.Entities.Relationship.InterfaceForeignKey
{
    public interface IIndustryFK
    {
        long IndustryId { get; set; }
        Industry Industry { get; set; }
    }
}
