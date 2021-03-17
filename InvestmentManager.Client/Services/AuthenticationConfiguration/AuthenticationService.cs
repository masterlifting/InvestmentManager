using InvestmentManager.Client.Services.HttpService;
using InvestmentManager.Models;
using InvestmentManager.Models.Security;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.AuthenticationConfiguration
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly CustomHttpClient http;
        private readonly CustomAuthenticationStateProvider customAuthenticationState;

        public AuthenticationService( CustomHttpClient http , CustomAuthenticationStateProvider customAuthenticationState)
        {
            this.http = http;
            this.customAuthenticationState = customAuthenticationState;
        }

        public async Task<LoginResult> LoginAsync(LoginModel model)
        {
            var result = await http.PostAsync<LoginResult, LoginModel>("security/login/", model);
            
            if (result.IsSuccess)
                await customAuthenticationState.SetTokenAsync(result.Token, result.Expiry);

            return result;
        }
        public async Task<BaseActionResult> RegisterAsync(RegisterModel model) =>
            await http.PostAsync<BaseActionResult, RegisterModel>("security/register/", model);
        public async Task LogoutAsync() => await customAuthenticationState.SetTokenAsync(null);
    }
}
