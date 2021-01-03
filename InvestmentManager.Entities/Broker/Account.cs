using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Calculate;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Broker
{
    public class Account : BaseEntity
    {
        [StringLength(100)]
        [Required]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string UserId { get; set; }

        public virtual IEnumerable<Dividend> Dividends { get; set; }
        public virtual IEnumerable<AccountTransaction> AccountTransactions { get; set; }
        public virtual IEnumerable<StockTransaction> StockTransactions { get; set; }
        public virtual IEnumerable<Comission> Comissions { get; set; }
        public virtual IEnumerable<ExchangeRate> ExchangeRates { get; set; }

        public virtual IEnumerable<ExchangeRateSummary> ExchangeRateSummaries { get; set; }
        public virtual IEnumerable<ComissionSummary> ComissionSummaries { get; set; }
        public virtual IEnumerable<AccountSummary> AccountSummaries { get; set; }
        public virtual IEnumerable<CompanySummary> CompanySummaries { get; set; }
        public virtual IEnumerable<DividendSummary> DividendSummaries { get; set; }
    }
}
