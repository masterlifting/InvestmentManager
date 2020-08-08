using InvestmentManager.Calculator.ConfigurationBinding;
using InvestmentManager.Calculator.Interfaces;
using InvestmentManager.Entities.Calculate;
using System.Collections.Generic;
using System.Linq;

namespace InvestmentManager.Calculator.Implimentations
{
    internal class CoefficientCalculate : BaseCalculate, ICoefficientCalculate
    {
        private readonly IEnumerable<decimal> profitabilityCollection;
        private readonly IEnumerable<decimal> roaCollection;
        private readonly IEnumerable<decimal> roeCollection;
        private readonly IEnumerable<decimal> peCollection;
        private readonly IEnumerable<decimal> pbCollection;
        private readonly IEnumerable<decimal> debtLoadCollection;
        private readonly IEnumerable<decimal> epsCollection;

        public CoefficientCalculate(IEnumerable<Coefficient> sortedCoefficients)
        {
            profitabilityCollection = sortedCoefficients.Where(x => x.Profitability != 0).Select(x => x.Profitability);
            roaCollection = sortedCoefficients.Where(x => x.ROA != 0).Select(x => x.ROA);
            roeCollection = sortedCoefficients.Where(x => x.ROE != 0).Select(x => x.ROE);
            epsCollection = sortedCoefficients.Where(x => x.EPS != 0).Select(x => x.EPS);

            peCollection = sortedCoefficients.Where(x => x.PE != 0).Select(x => x.PE);
            pbCollection = sortedCoefficients.Where(x => x.PB != 0).Select(x => x.PB);
            debtLoadCollection = sortedCoefficients.Where(x => x.DebtLoad != 0).Select(x => x.DebtLoad);
        }

        public decimal GetCoefficientAverage()
        {
            var positiveCollection = new List<decimal>
            {
                profitabilityCollection.Any() ? profitabilityCollection.Average() : 0
                , roeCollection.Any() ? roeCollection.Average() : 0
                , roaCollection.Any() ? roaCollection.Average() : 0
                , epsCollection.Any() ? epsCollection.Average() : 0
            };

            var peMoreZero = peCollection.Where(x => x > 0);
            var peLessZero = peCollection.Where(x => x < 0);
            var pbMoreZero = pbCollection.Where(x => x > 0);
            var pbLessZero = pbCollection.Where(x => x < 0);

            decimal weightAverage = WeightConfig.CoefficientAverage > 0 ? WeightConfig.CoefficientAverage : 1;

            decimal peCollectionResult = (peMoreZero.Any() ? peMoreZero.Average() : 0) + (peLessZero.Any() ? peLessZero.Count() * weightAverage : 0);
            decimal bpCollectionResult = (pbMoreZero.Any() ? pbMoreZero.Average() : 0) + (pbLessZero.Any() ? pbLessZero.Count() * weightAverage : 0);

            var negativeCollection = new List<decimal>() { peCollectionResult, bpCollectionResult, debtLoadCollection.Any() ? debtLoadCollection.Average() : 0 };

            return (positiveCollection.Sum() - negativeCollection.Sum()) * weightAverage;
        }
        public decimal GetCoefficientComparison()
        {
            var collectionPositive = new List<IEnumerable<decimal>>()
            {
                profitabilityCollection.Any() ? profitabilityCollection : new List<decimal>()
                , roeCollection.Any() ? roeCollection : new List<decimal>()
                , roaCollection.Any() ? roaCollection : new List<decimal>()
                , epsCollection.Any() ? epsCollection : new List<decimal>()
            };
            var collectionNegative = new List<IEnumerable<decimal>>()
            {
                 peCollection.Any() ? peCollection : new List<decimal>()
                , pbCollection.Any() ? pbCollection : new List<decimal>()
                , debtLoadCollection.Any() ? debtLoadCollection : new List<decimal>()
            };

            Weight = WeightConfig.CoefficientComparision > 0 ? WeightConfig.CoefficientComparision : 1;
            PositiveCollections = collectionPositive;
            NegativeCollections = collectionNegative;

            return CollectionComparison();
        }
    }
}
