using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Market;
using System;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Basic
{
    public abstract class BaseBroker : IBaseBroker
    {
        protected BaseBroker() => DateUpdate = DateTime.Now;
        [Key]
        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public DateTime DateOperation { get; set; }

        public long AccountId { get; set; }
        public virtual Account Account { get; set; }

        public long CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }
    }
}
