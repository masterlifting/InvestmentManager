using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Calculate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Market
{
    public class Company : BaseEntity
    {
        [StringLength(500)]
        [Required]
        public string Name { get; set; }
        public DateTime? DateSplit { get; set; }

        public long IndustryId { get; set; }
        public virtual Industry Industry { get; set; }

        public long SectorId { get; set; }
        public virtual Sector Sector { get; set; }

        public virtual IEnumerable<Ticker> Tickers { get; set; }
        public virtual IEnumerable<Isin> Isins { get; set; }

        public virtual ReportSource ReportSource { get; set; }

        public virtual IEnumerable<Report> Reports { get; set; }

        public virtual Rating Rating { get; set; }
        public virtual BuyRecommendation BuyRecommendation { get; set; }
        public virtual SellRecommendation SellRecommendation { get; set; }

        public virtual IEnumerable<CompanySummary> CompanySummaries { get; set; }
        public virtual IEnumerable<DividendSummary> DividendSummaries { get; set; }
    }
}
