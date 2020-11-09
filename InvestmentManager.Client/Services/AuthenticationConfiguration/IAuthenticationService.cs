using InvestmentManager.ViewModels.ResultModels;
using InvestmentManager.ViewModels.SecurityModels;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.AuthenticationConfiguration
{
    public interface IAuthenticationService
    {
        Task<ResultBaseModel> RegisterAsync(RegisterModel model);
        Task<LoginResult> LoginAsync(LoginModel model);
        Task LogoutAsync();
    }
}