using System;

namespace InvestmentManager.ViewModels.TransactionModels
{
    public class AccountTransactionModel
    {
        public string Account { get; set; }
        public string Status { get; set; }
        public DateTime DateTransaction { get; set; }
        public decimal Sum { get; set; }
        public long CurrencyId { get; set; }
    }
}
