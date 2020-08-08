using System;
using System.Collections.Generic;

namespace InvestmentManager.Web.Models.ChartModels
{
    public class BaseChartModel
    {
        public IEnumerable<KeyValuePair<DateTime, decimal>> Points { get; set; }
        public string XName { get; set; }
        public string YName { get; set; }
        public string Title { get; set; }
    }
}
