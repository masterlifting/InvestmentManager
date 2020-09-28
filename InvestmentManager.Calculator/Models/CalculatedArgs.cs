using InvestManager.Entities.Calculate;
using InvestManager.Entities.Market;
using System.Collections.Generic;

namespace InvestManager.Calculator.Models
{
    public class CalculatedArgs
    {
        public Rating CurrentRating { get; set; }
        public IEnumerable<Price> Prices { get; set; }
        public IEnumerable<Report> Reports { get; set; }
        public IEnumerable<Coefficient> Coefficients { get; set; }
    }
}
