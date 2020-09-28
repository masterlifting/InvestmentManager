using InvestManager.DomainModels.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestManager.ViewModels.ReportModels
{
    public class NewCompanyReportModel
    {
        public string CompanyName { get; set; }
        public IList<NewReportModel> Reports { get; set; } = new List<NewReportModel>();
    }
    public class NewReportModel
    {
        decimal revenue;
        decimal netProfit;
        decimal grossProfit;
        decimal cashFlow;
        decimal assets;
        decimal turnover;
        decimal shareCapital;
        decimal dividend;
        decimal obligation;
        decimal longTermDebt;

        public long ReportId { get; set; }
        public DateTime DateReport { get; set; }
        [Zero(ErrorMessage = "Выпущенный акций должно быть больше 0")]
        [Range(1, long.MaxValue, ErrorMessage = "Выпущенных акций должно быть больше 0")]
        public long StockVolume { get; set; }
        [Zero(ErrorMessage = "Выручка не должна быть равна 0")]
        public decimal Revenue { get => revenue; set => revenue = Math.Round(value, 2); }
        [Zero(ErrorMessage = "Чистая прибыль не должна быть равна 0")]
        public decimal NetProfit { get => netProfit; set => netProfit = Math.Round(value, 2); }
        public decimal GrossProfit { get => grossProfit; set => grossProfit = Math.Round(value, 2); }
        public decimal CashFlow { get => cashFlow; set => cashFlow = Math.Round(value, 2); }
        [Zero(ErrorMessage = "Денежный поток не должен быть равен 0")]
        public decimal Assets { get => assets; set => assets = Math.Round(value, 2); }
        public decimal Turnover { get => turnover; set => turnover = Math.Round(value, 2); }
        [Zero(ErrorMessage = "Акционерный капитал не должен быть равен 0")]
        public decimal ShareCapital { get => shareCapital; set => shareCapital = Math.Round(value, 2); }
        public decimal Dividend { get => dividend; set => dividend = Math.Round(value, 2); }
        public decimal Obligation { get => obligation; set => obligation = Math.Round(value, 2); }
        public decimal LongTermDebt { get => longTermDebt; set => longTermDebt = Math.Round(value, 2); }
    }
}
