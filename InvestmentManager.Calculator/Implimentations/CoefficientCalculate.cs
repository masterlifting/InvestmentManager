using InvestmentManager.Calculator.ConfigurationBinding;
using InvestmentManager.Calculator.Interfaces;
using InvestmentManager.Entities.Calculate;
using System.Collections.Generic;
using System.Linq;

namespace InvestmentManager.Calculator.Implimentations
{
    internal class CoefficientCalculate : BaseCalculate, ICoefficientCalculate
    {
        private readonly List<decimal> profitabilityCollection;
        private readonly List<decimal> roaCollection;
        private readonly List<decimal> roeCollection;
        private readonly List<decimal> peCollection;
        private readonly List<decimal> pbCollection;
        private readonly List<decimal> debtLoadCollection;
        private readonly List<decimal> epsCollection;

        public CoefficientCalculate(List<Coefficient> sortedCoefficients)
        {
            profitabilityCollection = sortedCoefficients.Where(x => x.Profitability != 0).Select(x => x.Profitability).ToList();
            roaCollection = sortedCoefficients.Where(x => x.ROA != 0).Select(x => x.ROA).ToList();
            roeCollection = sortedCoefficients.Where(x => x.ROE != 0).Select(x => x.ROE).ToList();
            epsCollection = sortedCoefficients.Where(x => x.EPS != 0).Select(x => x.EPS).ToList();

            peCollection = sortedCoefficients.Where(x => x.PE != 0).Select(x => x.PE).ToList();
            pbCollection = sortedCoefficients.Where(x => x.PB != 0).Select(x => x.PB).ToList();
            debtLoadCollection = sortedCoefficients.Where(x => x.DebtLoad != 0).Select(x => x.DebtLoad).ToList();
        }

        public decimal? GetCoefficientAverage()
        {
            decimal weightAverage = WeightConfig.CoefficientAverage > 0 ? WeightConfig.CoefficientAverage : 1;

            var positiveCollection = new List<decimal> { profitabilityCollection.Average(), roeCollection.Average(), roaCollection.Average(), epsCollection.Average() };

            var peMoreZero = peCollection.Where(x => x > 0).ToList();
            var peLessZero = peCollection.Where(x => x < 0).ToList();
            var pbMoreZero = pbCollection.Where(x => x > 0).ToList();
            var pbLessZero = pbCollection.Where(x => x < 0).ToList();

            decimal peCollectionResult = peLessZero.Count * weightAverage + (peMoreZero.Any() ? peMoreZero.Average() : 0);
            decimal bpCollectionResult = pbLessZero.Count * weightAverage + (pbMoreZero.Any() ? pbMoreZero.Average() : 0);

            var negativeCollection = new List<decimal>() { peCollectionResult, bpCollectionResult, debtLoadCollection.Average() };

            var result = (positiveCollection.Sum() - negativeCollection.Sum()) * weightAverage;

            return result != 0 ? result : null;
        }
        public decimal? GetCoefficientComparison()
        {
            Weight = WeightConfig.CoefficientComparision > 0 ? WeightConfig.CoefficientComparision : 1;
            PositiveCollections = new List<List<decimal>>() { profitabilityCollection, roeCollection, roaCollection, epsCollection };
            NegativeCollections = new List<List<decimal>>() { peCollection, pbCollection, debtLoadCollection };
            return CollectionComparison();
        }
    }
}
