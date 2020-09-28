using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Relationship.InterfaceForeignKey
{
    public interface IIndustryFK
    {
        long IndustryId { get; set; }
        Industry Industry { get; set; }
    }
}
