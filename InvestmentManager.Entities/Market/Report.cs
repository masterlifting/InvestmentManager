﻿using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Calculate;
using InvestmentManager.Entities.Relationship.InterfaceForeignKey;
using InvestmentManager.Entities.Relationship.InterfaceNavigationProperty;

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Market
{
    public class Report : BaseEntity, ICompanyFK, ICoefficientNP
    {
        public Report() => IsChecked = false;

        public DateTime DateReport { get; set; }
        public bool IsChecked { get; set; }

        public long StockVolume { get; set; }

        [Column(TypeName = "Decimal(18,4)")]
        public decimal Revenue { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal NetProfit { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal GrossProfit { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal CashFlow { get; set; }

        [Column(TypeName = "Decimal(18,4)")]
        public decimal Assets { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Turnover { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal ShareCapital { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Dividends { get; set; }

        [Column(TypeName = "Decimal(18,4)")]
        public decimal Obligations { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal LongTermDebt { get; set; }

        #region Relationship
        public Coefficient Coefficient { get; set; }

        public long CompanyId { get; set; }
        public Company Company { get; set; }
        #endregion
    }
}
