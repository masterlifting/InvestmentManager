using System;

namespace InvestmentManager.Models.Additional
{
    public class CBRF
    {
        public DateTime Date { get; set; }
        public Valute Valute { get; set; } = new();
    }

    public class Valute
    {
        public USD USD { get; set; } = new();
        public EUR EUR { get; set; } = new();

    }

    public class USD
    {
        public decimal Value { get; set; }
    }
    public class EUR
    {
        public decimal Value { get; set; }
    }
}
