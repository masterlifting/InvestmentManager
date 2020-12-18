using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Calculate;
using InvestmentManager.Entities.Relationship.InterfaceNavigationProperty;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Market
{
    public class Currency : BaseEntity, IPriceNP, IDividendNP, IAccountTransactionNP, IStockTransactioNP, IComissionNP, IExchaneRateNP
    {
        [StringLength(10)]
        [Required]
        public string Name { get; set; }

        public virtual IEnumerable<Price> Prices { get; set; }
        public virtual IEnumerable<Dividend> Dividends { get; set; }
        public virtual IEnumerable<AccountTransaction> AccountTransactions { get; set; }
        public virtual IEnumerable<StockTransaction> StockTransactions { get; set; }
        public virtual IEnumerable<Comission> Comissions { get; set; }
        public virtual IEnumerable<ExchangeRate> ExchangeRates { get; set; }

        public virtual IEnumerable<CompanySummary> CompanySummaries { get; set; }
        public virtual IEnumerable<AccountSummary> AccountSummaries { get; set; }
        public virtual IEnumerable<ExchangeRateSummary> ExchangeRateSummaries { get; set; }
        public virtual IEnumerable<DividendSummary> DividendSummaries { get; set; }
        public virtual IEnumerable<ComissionSummary> ComissionSummaries { get; set; }
    }
}
