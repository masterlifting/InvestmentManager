using Blazored.LocalStorage;
using InvestManager.ViewModels.AuthenticationModels;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace InvestManager.Client.AuthConfiguration
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient httpClient;
        private readonly AuthenticationStateProvider authenticationStateProvider;
        private readonly ILocalStorageService localStorage;

        public AuthService(
            HttpClient httpClient
            , AuthenticationStateProvider authenticationStateProvider
            , ILocalStorageService localStorage)
        {
            this.httpClient = httpClient;
            this.authenticationStateProvider = authenticationStateProvider;
            this.localStorage = localStorage;
        }

        public async Task<RegisterResult> RegisterAsync(RegisterModel model)
        {
            var response = await httpClient.PostAsJsonAsync("accounts", model).ConfigureAwait(false);
            return JsonSerializer.Deserialize<RegisterResult>(await response.Content.ReadAsStringAsync().ConfigureAwait(false), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        public async Task<LoginResult> LoginAsync(LoginModel model)
        {
            var response = await httpClient.PostAsJsonAsync("login", model).ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<LoginResult>(await response.Content.ReadAsStringAsync().ConfigureAwait(false), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!response.IsSuccessStatusCode)
                return result;

            await localStorage.SetItemAsync("authToken", result.Token).ConfigureAwait(false);
            ((ApiAuthenticationStateProvider)authenticationStateProvider).MarkUserAsAuthenticated(model.Email);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Token);

            return result;
        }
        public async Task LogoutAsync()
        {
            await localStorage.RemoveItemAsync("authToken").ConfigureAwait(false);
            ((ApiAuthenticationStateProvider)authenticationStateProvider).MarkUserAsLogout();
            httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
