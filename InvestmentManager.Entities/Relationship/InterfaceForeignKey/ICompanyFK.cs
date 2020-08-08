using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Relationship.InterfaceForeignKey
{
    public interface ICompanyFK
    {
        long CompanyId { get; set; }
        Company Company { get; set; }
    }
}
