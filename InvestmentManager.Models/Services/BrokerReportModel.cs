using InvestmentManager.Models.EntityModels;
using System.Collections.Generic;
using static InvestmentManager.Models.Enums;

namespace InvestmentManager.Models.Services
{
    public class BrokerReportModel
    {
        public List<BrokerReportSuccessedModel> Reports { get; set; } = new List<BrokerReportSuccessedModel>();
        public List<BrokerReportErrorModel> Errors { get; set; } = new List<BrokerReportErrorModel>();
    }
    public class BrokerReportSuccessedModel
    {
        public long AccountId { get; set; }
        public List<ComissionModel> Comissions { get; set; }
        public List<StockTransactionModel> StockTransactions { get; set; }
        public List<DividendModel> Dividends { get; set; }
        public List<AccountTransactionModel> AccountTransactions { get; set; }
        public List<ExchangeRateModel> ExchangeRates { get; set; }
    }
    public class BrokerReportErrorModel
    {
        public BrokerReportErrorTypes ErrorType { get; set; }
        public string ErrorValue { get; set; }
    }
}
