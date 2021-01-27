using System;

namespace InvestmentManager.Entities.Market
{
    public class Weekend
    {
        public int Id { get; set; }

        public DateTime ExchangeWeekend { get; set; }

        public virtual Exchange Exchange { get; set; }
        public long ExchangeId { get; set; }
    }
}
