using InvestmentManager.Client.Services.HttpService;
using InvestmentManager.Models.Security;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.AuthenticationConfiguration
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly API api;
        private readonly CustomAuthenticationStateProvider customAuthenticationState;

        public AuthenticationService( API api , CustomAuthenticationStateProvider customAuthenticationState)
        {
            this.api = api;
            this.customAuthenticationState = customAuthenticationState;
        }

        public async Task<AuthResult> LoginAsync(LoginModel model)
        {
            var result = await api.Security.Value.LoginAsync(model);
            
            if (result.IsSuccess)
                await customAuthenticationState.SetTokenAsync(result.Token, result.Expiry);

            return result;
        }
        public async Task<AuthResult> RegisterAsync(RegisterModel model)
        {
            var result = await api.Security.Value.RegisterAsync(model);

            if (result.IsSuccess)
                await customAuthenticationState.SetTokenAsync(result.Token, result.Expiry);

            return result;
        }

        public async Task LogoutAsync() => await customAuthenticationState.SetTokenAsync(null);
    }
}
