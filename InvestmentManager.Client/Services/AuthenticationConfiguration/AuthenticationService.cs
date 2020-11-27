using InvestmentManager.Client.Services.HttpService;
using InvestmentManager.Models;
using InvestmentManager.Models.Security;
using System.Net.Http;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.AuthenticationConfiguration
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient httpClient;
        private readonly CustomHttpClient http;
        private readonly CustomAuthenticationStateProvider customAuthenticationState;

        public AuthenticationService(
            HttpClient httpClient
            , CustomHttpClient http
            , CustomAuthenticationStateProvider customAuthenticationState)
        {
            this.httpClient = httpClient;
            this.http = http;
            this.customAuthenticationState = customAuthenticationState;
        }

        public async Task<LoginResult> LoginAsync(LoginModel model)
        {
            var result = await http.PostAsync<LoginResult, LoginModel>("security/login/", model).ConfigureAwait(false);
            
            if (result.IsSuccess)
                await customAuthenticationState.SetTokenAsync(result.Token, result.Expiry).ConfigureAwait(false);

            return result;
        }
        public async Task<BaseResult> RegisterAsync(RegisterModel model) =>
            await http.PostAsync<BaseResult, RegisterModel>("security/register/", model).ConfigureAwait(false);
        public async Task LogoutAsync() => await customAuthenticationState.SetTokenAsync(null).ConfigureAwait(false);
    }
}
