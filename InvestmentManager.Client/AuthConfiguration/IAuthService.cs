using InvestmentManager.ViewModels.AuthenticationModels;
using System.Threading.Tasks;

namespace InvestmentManager.Client.AuthConfiguration
{
    public interface IAuthService
    {
        Task<RegisterResult> RegisterAsync(RegisterModel model);
        Task<LoginResult> LoginAsync(LoginModel model);
        Task LogoutAsync();
    }
}