using System;

namespace InvestmentManager.ViewModels.TransactionModels
{
    public class StockTransactionDetailModel
    {
        public string TickerName { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime DateTransaction { get; set; }
    }
}
