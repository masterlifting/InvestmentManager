using InvestmentManager.ViewModels.ErrorModels;
using InvestmentManager.ViewModels.SecurityModels;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.AuthConfiguration
{
    public interface IAuthService
    {
        Task<ErrorBaseModel> RegisterAsync(RegisterModel model);
        Task<LoginResult> LoginAsync(LoginModel model);
        Task LogoutAsync();
    }
}