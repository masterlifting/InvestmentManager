using InvestmentManager.Calculator.ConfigurationBinding;
using InvestmentManager.Calculator.Interfaces;
using InvestmentManager.Entities.Market;
using System.Collections.Generic;
using System.Linq;

namespace InvestmentManager.Calculator.Implimentations
{
    internal class PriceCalculate : BaseCalculate, IPriceCalculate
    {
        private readonly IEnumerable<Price> prices;
        public PriceCalculate(IEnumerable<Price> orderedPrices) => prices = orderedPrices;

        public decimal GetPricieComporision()
        {
            var thisPriceList = prices.Where(x => x.Value != 0).Select(x => x.Value);
            var collection = thisPriceList.Any() ? thisPriceList : new List<decimal>();

            Weight = WeightConfig.PriceComparision > 0 ? WeightConfig.PriceComparision : 1;
            PositiveCollections = new List<IEnumerable<decimal>> { collection };

            return CollectionComparison();
        }
    }
}
