using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Relationship.InterfaceForeignKey
{
    interface IReportFK
    {
        long ReportId { get; set; }
        Report Report { get; set; }
    }
}
