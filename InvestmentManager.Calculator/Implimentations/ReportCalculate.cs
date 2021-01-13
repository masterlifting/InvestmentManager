using InvestmentManager.Calculator.ConfigurationBinding;
using InvestmentManager.Calculator.Interfaces;
using InvestmentManager.Entities.Market;
using System.Collections.Generic;
using System.Linq;

namespace InvestmentManager.Calculator.Implimentations
{
    internal class ReportCalculate : BaseCalculate, IReportCalculate
    {
        private readonly List<Report> reports;
        public ReportCalculate(IEnumerable<Report> orderedReports) => reports = orderedReports.ToList();

        public decimal? GetCashFlowBalance(decimal maxPercent = 10)
        {
            decimal weight = WeightConfig.ReportCashFlow > 0 ? WeightConfig.ReportCashFlow : 1;

            decimal[] cashFlowCollection = reports.Where(x => x.CashFlow != 0).Select(x => x.CashFlow).ToArray();

            decimal result = 0;
            decimal comparisonCount = cashFlowCollection.Length;

            if (comparisonCount > 0)
            {
                decimal stepPercent = maxPercent / comparisonCount;

                for (int i = 0; i < comparisonCount; i++)
                {
                    if (cashFlowCollection[i] > 0)
                        result += stepPercent;
                    else
                        result -= stepPercent;
                }

                return result * weight;
            }
            else return null;
        }
        public decimal? GetReportComporision()
        {
            Weight = WeightConfig.ReportComparision > 0 ? WeightConfig.ReportComparision : 1;

            // positive collection
            var revenue = reports.Where(x => x.Revenue != 0).Select(x => x.Revenue).ToList();
            var netProfit = reports.Where(x => x.NetProfit != 0).Select(x => x.NetProfit).ToList();
            var grossProfit = reports.Where(x => x.GrossProfit != 0).Select(x => x.GrossProfit).ToList();
            var assets = reports.Where(x => x.Assets != 0).Select(x => x.Assets).ToList();
            var turnover = reports.Where(x => x.Turnover != 0).Select(x => x.Turnover).ToList();
            var shareCapital = reports.Where(x => x.ShareCapital != 0).Select(x => x.ShareCapital).ToList();
            var dividends = reports.Where(x => x.Dividends != 0).Select(x => x.Dividends).ToList();
            // negative collection
            var obligations = reports.Where(x => x.Obligations != 0).Select(x => x.Obligations).ToList();
            var longTermDebt = reports.Where(x => x.LongTermDebt != 0).Select(x => x.LongTermDebt).ToList();

            PositiveCollections = new List<List<decimal>>() { revenue, netProfit, grossProfit, assets, turnover, shareCapital, dividends };
            NegativeCollections = new List<List<decimal>>() { obligations, longTermDebt };
            return CollectionComparison();
        }
    }
}
