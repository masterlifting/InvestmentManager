using InvestmentManager.Client.Services.HttpService;
using InvestmentManager.ViewModels;
using InvestmentManager.ViewModels.ResultModels;
using InvestmentManager.ViewModels.SecurityModels;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.AuthenticationConfiguration
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly CustomHttpClient http;
        private readonly CustomAuthenticationStateProvider customAuthenticationState;

        public AuthenticationService(
            CustomHttpClient http
            , CustomAuthenticationStateProvider customAuthenticationState)
        {
            this.http = http;
            this.customAuthenticationState = customAuthenticationState;
        }

        public async Task<LoginResult> LoginAsync(LoginModel model)
        {
            var result = await http.PostAsModelAsync<LoginResult, LoginModel>(Routes.C.Security + "/login", model, true).ConfigureAwait(false);
            
            if (result.Successful)
                await customAuthenticationState.SetTokenAsync(result.Token, result.Expiry).ConfigureAwait(false);

            return result;
        }
        public async Task<ResultBaseModel> RegisterAsync(RegisterModel model) =>
            await http.PostAsModelAsync<ResultBaseModel, RegisterModel>(Routes.C.Security + "/register", model, true).ConfigureAwait(false);
        public async Task LogoutAsync() => await customAuthenticationState.SetTokenAsync(null).ConfigureAwait(false);
    }
}
