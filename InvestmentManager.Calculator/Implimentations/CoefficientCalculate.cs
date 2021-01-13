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

        public CoefficientCalculate(IEnumerable<Coefficient> sortedCoefficients)
        {
            profitabilityCollection = sortedCoefficients.Select(x => x.Profitability).Where(x => x != default).ToList();
            roaCollection = sortedCoefficients.Select(x => x.ROA).Where(x => x != default).ToList();
            roeCollection = sortedCoefficients.Select(x => x.ROE).Where(x => x != default).ToList();
            epsCollection = sortedCoefficients.Select(x => x.EPS).Where(x => x != default).ToList();
            peCollection = sortedCoefficients.Select(x => x.PE).Where(x => x != default).ToList();
            pbCollection = sortedCoefficients.Select(x => x.PB).Where(x => x != default).ToList();
            debtLoadCollection = sortedCoefficients.Select(x => x.DebtLoad).Where(x => x != default).ToList();
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
