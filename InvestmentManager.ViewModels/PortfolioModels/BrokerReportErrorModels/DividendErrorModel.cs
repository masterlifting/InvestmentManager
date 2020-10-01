﻿using InvestmentManager.DomainModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.ViewModels.PortfolioModels.BrokerReportErrorModels
{
    public class DividendErrorModel
    {
        public IEnumerable<CompanyD> Companies { get; set; } = new List<CompanyD>();
    }
    public class DividendErrorResultModel
    {
        [StringLength(100, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string IdentifierName { get; set; }
        [StringLength(5, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string CompanyId { get; set; }
    }
}