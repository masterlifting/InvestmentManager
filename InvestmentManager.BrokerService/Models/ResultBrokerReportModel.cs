using System.Collections.Generic;

namespace InvestManager.BrokerService.Models
{
    public class ResultBrokerReportModel
    {
        public IEnumerable<EntityReportModel> Reports { get; set; } = new List<EntityReportModel>();
        public IEnumerable<ErrorReportModel> Errors { get; set; } = new List<ErrorReportModel>();
    }
}
