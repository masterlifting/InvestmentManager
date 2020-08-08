using System.Collections.Generic;
using System.Linq;

namespace InvestmentManager.Web.Models.FinancialModels
{
    public class ReportComponentModel
    {
        public ReportComponentModel(IEnumerable<ReportBodyModel> reportBodyModels)
        {
            if (reportBodyModels != null)
                ReportBodyModels = reportBodyModels.ToList();
            else
                ReportBodyModels = new List<ReportBodyModel>();
        }

        public long CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int LastYear { get; set; }
        public int LastQuarter { get; set; }
        public int TotalCount { get; set; }

        public List<ReportBodyModel> ReportBodyModels { get; }
    }
}
