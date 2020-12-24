using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace InvestmentManager.Calculator
{
    internal abstract class BaseCalculate
    {
        private protected static readonly CultureInfo cultureConfig = CultureInfo.InvariantCulture;
        private protected const NumberStyles styleConfig = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;

        public decimal Weight { get; private protected set; }
        public IEnumerable<IEnumerable<decimal>> PositiveCollections { get; private protected set; } = null;
        public IEnumerable<IEnumerable<decimal>> NegativeCollections { get; private protected set; } = null;

        private protected decimal? CollectionComparison()
        {
            var result = new List<decimal>();

            if (PositiveCollections is not null && PositiveCollections.Any())
                foreach (var pc in PositiveCollections.Where(x => x.Any()))
                    result.Add(CalculateNextToPreviousPercentChange(pc, 100));

            if (NegativeCollections is not null && NegativeCollections.Any())
                foreach (var nc in NegativeCollections.Where(x => x.Any()))
                    result.Add(CalculateNextToPreviousPercentChange(nc, -100));

            var _result = result.Where(x => x != 0);
            return _result.Any() ? _result.Average() * Weight : null;
        }

        // Сравнение изменения усреднений даух значений в процентном отношении следующего элемента к предыдущему
        private static decimal CalculateNextToPreviousPercentChange(IEnumerable<decimal> collection, int typeCalculateValue)
        {
            decimal result = 0;
            var avgList = new List<decimal>();
            decimal[] _collection = collection.ToArray();

            if (_collection.Length > 1)
            {
                for (int j = 1; j < _collection.Length; j++)
                {
                    avgList.Add((_collection[j] + _collection[j - 1]) * 0.5m);
                }
                for (int j = 1; j < avgList.Count; j++)
                {
                    if (typeCalculateValue < 0 & (avgList[j - 1] < 0 || avgList[j] < 0))
                    {
                        result += 0.0001m;
                    }
                    else if (avgList[j - 1] != 0)
                    {
                        decimal tempResult = (((avgList[j] - avgList[j - 1]) / Math.Abs(avgList[j - 1])) * typeCalculateValue);
                        result += tempResult != 0 ? tempResult : 0.0001m;
                    }
                    else if (avgList[j - 1] == 0)
                    {
                        avgList[j] += 1;
                        avgList[j - 1] += 1;

                        result += (((avgList[j] - avgList[j - 1]) / Math.Abs(avgList[j - 1])) * typeCalculateValue);

                        avgList[j] -= 1;
                        avgList[j - 1] -= 1;
                    }
                }
            }
            return result != 0 ? result / (_collection.Length - 2) : result;
        }
    }
}
