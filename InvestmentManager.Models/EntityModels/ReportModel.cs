using InvestmentManager.Entities.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Models.EntityModels
{
    public class ReportModel
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public bool IsChecked { get; set; }
        public int Quarter { get; set; }

        public DateTime DateReport { get; set; }
        [Zero(ErrorMessage = "Issued shares must be greater than 0")]
        [Range(1, long.MaxValue, ErrorMessage = "Issued shares must be greater than 0")]
        public long StockVolume { get; set; }
        [Zero(ErrorMessage = "Revenue must not be 0")]
        public decimal Revenue { get; set; }
        [Zero(ErrorMessage = "Net profit should not be equal to 0")]
        public decimal NetProfit { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal CashFlow { get; set; }
        [Zero(ErrorMessage = "Cash flow must not be 0")]
        public decimal Assets { get; set; }
        public decimal Turnover { get; set; }
        [Zero(ErrorMessage = "Share capital must not be equal to 0")]
        public decimal ShareCapital { get; set; }
        public decimal Dividend { get; set; }
        public decimal Obligation { get; set; }
        public decimal LongTermDebt { get; set; }
    }
}
