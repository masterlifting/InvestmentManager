﻿using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.ViewModels.ReportModels.BrokerReportModels
{
    public class AccountErrorForm
    {
        [StringLength(50, ErrorMessage = DefaultData.errorLenght,MinimumLength =20), Required(ErrorMessage = DefaultData.errorRequired)]
        public string AccountName { get; set; }
    }
}
