using InvestmentManager.Calculator.ConfigurationBinding;
using InvestmentManager.Calculator.Interfaces;
using InvestmentManager.Entities.Market;
using System.Collections.Generic;
using System.Linq;

namespace InvestmentManager.Calculator.Implimentations
{
    internal class ReportCalculate : BaseCalculate, IReportCalculate
    {
        private readonly IEnumerable<Report> reports;
        public ReportCalculate(IEnumerable<Report> orderedReports) => reports = orderedReports;

        public decimal GetCashFlowBalance(decimal maxPercent = 10)
        {
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

                result = result == 0 ? 0.0001m : result;
            }

            return result * (WeightConfig.ReportCashFlow > 0 ? WeightConfig.ReportCashFlow : 1);
        }
        public decimal GetReportComporision()
        {
            // positive collection
            var revenue = reports.Where(x => x.Revenue != 0).Select(x => x.Revenue).ToArray();
            var netProfit = reports.Where(x => x.NetProfit != 0).Select(x => x.NetProfit).ToArray();
            var grossProfit = reports.Where(x => x.GrossProfit != 0).Select(x => x.GrossProfit).ToArray();
            var assets = reports.Where(x => x.Assets != 0).Select(x => x.Assets).ToArray();
            var turnover = reports.Where(x => x.Turnover != 0).Select(x => x.Turnover).ToArray();
            var shareCapital = reports.Where(x => x.ShareCapital != 0).Select(x => x.ShareCapital).ToArray();
            var dividends = reports.Where(x => x.Dividends != 0).Select(x => x.Dividends).ToArray();
            // negetive collection
            var obligations = reports.Where(x => x.Obligations != 0).Select(x => x.Obligations).ToArray();
            var longTermDebt = reports.Where(x => x.LongTermDebt != 0).Select(x => x.LongTermDebt).ToArray();

            var collectionPositive = new List<decimal[]>() { revenue, netProfit, grossProfit, assets, turnover, shareCapital, dividends };
            var collectionNegative = new List<decimal[]>() { obligations, longTermDebt };

            Weight = WeightConfig.ReportComparision > 0 ? WeightConfig.ReportComparision : 1;
            PositiveCollections = collectionPositive;
            NegativeCollections = collectionNegative;

            return CollectionComparison();
        }
    }
}
