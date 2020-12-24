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
        public PriceCalculate(List<Price> orderedPrices) => prices = orderedPrices.Where(x => x.Value != 0).Select(x => x.Value).ToList();

        public decimal? GetPricieComporision()
        {
            Weight = WeightConfig.PriceComparision > 0 ? WeightConfig.PriceComparision : 1;
            PositiveCollections = new List<List<decimal>> { prices };
            return CollectionComparison();
        }
    }
}
