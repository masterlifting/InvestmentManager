using InvestManager.ViewModels.AuthenticationModels;
using System.Threading.Tasks;

namespace InvestManager.Client.AuthConfiguration
{
    public interface IAuthService
    {
        Task<RegisterResult> RegisterAsync(RegisterModel model);
        Task<LoginResult> LoginAsync(LoginModel model);
        Task LogoutAsync();
    }
}