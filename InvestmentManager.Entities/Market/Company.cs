using InvestManager.Entities.Basic;
using InvestManager.Entities.Calculate;
using InvestManager.Entities.Relationship.InterfaceForeignKey;
using InvestManager.Entities.Relationship.InterfaceNavigationProperty;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestManager.Entities.Market
{
    public class Company : BaseEntity
        , IIndustryFK
        , ISectorFK
        , ITickerNP
        , IReportNP
        , IRatingNP
        , IReportSourceNP
        , IBuyRecommendationNP
        , ISellRecommendationNP
        , IIsinNP
    {
        [StringLength(500)]
        [Required]
        public string Name { get; set; }

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
    }
}
