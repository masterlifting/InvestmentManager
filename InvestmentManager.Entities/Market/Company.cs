using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Calculate;
using InvestmentManager.Entities.Relationship.InterfaceForeignKey;
using InvestmentManager.Entities.Relationship.InterfaceNavigationProperty;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Market
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
        public Industry Industry { get; set; }

        public long SectorId { get; set; }
        public Sector Sector { get; set; }

        public List<Ticker> Tickers { get; set; }
        public List<Isin> Isins { get; set; }

        public ReportSource ReportSource { get; set; }

        public List<Report> Reports { get; set; }

        public Rating Rating { get; set; }
        public BuyRecommendation BuyRecommendation { get; set; }
        public SellRecommendation SellRecommendation { get; set; }
    }
}
