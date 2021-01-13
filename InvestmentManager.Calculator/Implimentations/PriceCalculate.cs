using InvestmentManager.Calculator.ConfigurationBinding;
using InvestmentManager.Calculator.Interfaces;
using InvestmentManager.Entities.Market;
using System.Collections.Generic;
using System.Linq;

namespace InvestmentManager.Calculator.Implimentations
{
    internal class PriceCalculate : BaseCalculate, IPriceCalculate
    {
        private readonly List<decimal> prices;
        public PriceCalculate(IEnumerable<Price> orderedPrices) => prices = orderedPrices.Select(x => x.Value).Where(x => x != default).ToList();

        public decimal? GetPricieComporision()
        {
            Weight = WeightConfig.PriceComparision > 0 ? WeightConfig.PriceComparision : 1;
            PositiveCollections = new List<List<decimal>> { prices };
            return CollectionComparison();
        }
    }
}
