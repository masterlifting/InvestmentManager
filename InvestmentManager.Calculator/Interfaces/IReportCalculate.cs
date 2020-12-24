namespace InvestmentManager.Calculator.Interfaces
{
    public interface IReportCalculate
    {
        decimal? GetReportComporision();
        decimal? GetCashFlowBalance(decimal maxPercent);
    }
}
