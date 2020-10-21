using Blazored.LocalStorage;
using InvestmentManager.Client.Services.NotificationService;
using InvestmentManager.ViewModels;
using InvestmentManager.ViewModels.ErrorModels;
using InvestmentManager.ViewModels.SecurityModels;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.AuthConfiguration
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient httpClient;
        private readonly AuthenticationStateProvider authenticationStateProvider;
        private readonly ILocalStorageService localStorage;
        private readonly Notification notification;

        public AuthService(
            HttpClient httpClient
            , AuthenticationStateProvider authenticationStateProvider
            , ILocalStorageService localStorage
            , Notification notification)
        {
            this.httpClient = httpClient;
            this.authenticationStateProvider = authenticationStateProvider;
            this.localStorage = localStorage;
            this.notification = notification;
        }

        public async Task<ErrorBaseModel> RegisterAsync(RegisterModel model)
        {
            notification.LoadStart();
            var response = await httpClient.PostAsJsonAsync(RouteName.security + "/register", model).ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<ErrorBaseModel>(await response.Content.ReadAsStringAsync().ConfigureAwait(false), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            notification.LoadStop();
            return result;
        }
        public async Task<LoginResult> LoginAsync(LoginModel model)
        {
            notification.LoadStart();
            var response = await httpClient.PostAsJsonAsync(RouteName.security + "/login", model).ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<LoginResult>(await response.Content.ReadAsStringAsync().ConfigureAwait(false), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            notification.LoadStop();

            if (!response.IsSuccessStatusCode)
                return result;

            await localStorage.SetItemAsync("authToken", result.Token).ConfigureAwait(false);
            ((ApiAuthenticationStateProvider)authenticationStateProvider).MarkUserAsAuthenticated(model?.Email);
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
