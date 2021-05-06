using System;

namespace InvestmentManager.ClientModels.CompanyModels
{
    public abstract class BrokerReportBase
    {
        public long CurrencyId { get; set; }
        public long AccountId { get; set; }
        public DateTime DateOperation { get; set; }
    }
}
