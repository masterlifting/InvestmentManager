using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Web.Models.PortfolioModels
{
    public class DividendErrorForm
    {
        [Display(Name = "Выбери для него компанию")]
        public SelectList Companies { get; set; }
        [Range(1, long.MaxValue)]
        public long CompanyId { get; set; }

        [Display(Name = "Введи ISIN или название компании")]
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Isin { get; set; }
    }
}
