using InvestManager.Entities.Market;

namespace InvestManager.Entities.Relationship.InterfaceForeignKey
{
    public interface ICompanyFK
    {
        long CompanyId { get; set; }
        Company Company { get; set; }
    }
}
