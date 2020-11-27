using System;

namespace InvestmentManager.Models.Additional
{
    public class CBRF
    {
        public CBRF() => Valute = new Valute();
        public DateTime Date { get; set; }
        public Valute Valute { get; set; }
    }

    public class Valute
    {
        public Valute() => USD = new USD();
        public USD USD { get; set; }
    }

    public class USD
    {
        public decimal Value { get; set; }
    }
}
