using InvestmentManager.Services.Interfaces;

namespace InvestmentManager.Services.Implimentations
{
    public class ConverterService : IConverterService
    {
        public int ConvertToQuarter(int month) => month switch
        {
            int x when x >= 1 && x < 4 => 1,
            int x when x >= 4 && x < 7 => 2,
            int x when x >= 7 && x < 10 => 3,
            int x when x >= 10 && x <= 12 => 4,
            _ => 0
        };
    }
}
