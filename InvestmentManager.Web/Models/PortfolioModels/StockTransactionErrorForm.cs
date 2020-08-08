using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Web.Models.PortfolioModels
{
    public class StockTransactionErrorForm
    {
        [Display(Name = "Выбери для него компанию")]
        public SelectList Companies { get; set; }
        [Range(1, long.MaxValue)]
        public long CompanyId { get; set; }

        [Display(Name = "Выбери площадку для этого тикера")]
        public SelectList Exchanges { get; set; }
        [Range(1, long.MaxValue)]
        public long ExchangeId { get; set; }

        [Display(Name = "Каким лотом торгуется тикер")]
        public SelectList Lots { get; set; }
        [Range(1, long.MaxValue)]
        public long LotId { get; set; }

        [Display(Name = "Введи тикер")]
        [Required]
        [StringLength(10, MinimumLength = 1)]
        public string TickerName { get; set; }
    }
}
