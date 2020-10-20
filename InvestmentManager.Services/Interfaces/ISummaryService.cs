using System.Threading.Tasks;

namespace InvestmentManager.Services.Interfaces
{
    public interface ISummaryService
    {
        Task<decimal> GetAccountSumAsync(long accountId);
    }
}
