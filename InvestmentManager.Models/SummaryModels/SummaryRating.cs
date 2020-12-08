using System;

namespace InvestmentManager.Models.SummaryModels
{
    public class SummaryRating
    {
        public DateTime DateUpdate { get; set; }
        public int PlaceCurrent { get; set; }
        public int PlaceTotal { get; set; }
        public decimal ValueTotal { get; set; }
    }
}
