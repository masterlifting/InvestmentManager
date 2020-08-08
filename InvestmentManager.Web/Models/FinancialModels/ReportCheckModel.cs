using InvestmentManager.Service.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Web.Models.FinancialModels
{
    public class ReportCheckModel
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }

        public string CompanyName { get; set; }

        [Display(Name = "Дата отчета")]
        public DateTime DateReport { get; set; }
        [Display(Name = "В обращении")]
        [Zero(ErrorMessage = "Акций в обращении не может не быть")]
        public long StockVolume { get; set; }

        [Display(Name = "Выручка")]
        [Zero(ErrorMessage = "Выручка не должна быть нулевой")]
        public decimal Revenue { get; set; }
        [Display(Name = "Чистая_прибыль")]
        public decimal NetProfit { get; set; }
        [Display(Name = "Валовая_прибыль")]
        public decimal GrossProfit { get; set; }
        [Display(Name = "Денежный_поток")]
        public decimal CashFlow { get; set; }

        [Display(Name = "Активы")]
        [Zero(ErrorMessage = "Активы не должны быть нулевыми")]
        public decimal Assets { get; set; }
        [Display(Name = "Оборот")]
        [Zero(ErrorMessage = "Оборот не должен быть нулевым")]
        public decimal Turnover { get; set; }
        [Display(Name = "Капитал")]
        [Zero(ErrorMessage = "Капитал не должен быть нулевым")]
        public decimal ShareCapital { get; set; }
        [Display(Name = "Дивиденды")]
        public decimal Dividends { get; set; }

        [Display(Name = "Обязательства")]
        public decimal Obligations { get; set; }
        [Display(Name = "Задолженность")]
        public decimal LongTermDebt { get; set; }
    }
}
