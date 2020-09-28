using InvestManager.Entities.Market;

namespace InvestManager.Entities.Relationship.InterfaceForeignKey
{
    interface IReportFK
    {
        long ReportId { get; set; }
        Report Report { get; set; }
    }
}
