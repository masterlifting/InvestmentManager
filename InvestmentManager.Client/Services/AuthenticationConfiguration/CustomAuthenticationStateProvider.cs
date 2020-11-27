using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.AuthenticationConfiguration
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService localStorage;
        public CustomAuthenticationStateProvider(ILocalStorageService localStorage) => this.localStorage = localStorage;

        public async Task SetTokenAsync(string token, DateTime expiry = default)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                await localStorage.RemoveItemAsync("authToken").ConfigureAwait(false);
                await localStorage.RemoveItemAsync("authTokenExpiry").ConfigureAwait(false);
            }
            else
            {
                await localStorage.SetItemAsync("authToken", token).ConfigureAwait(false);
                await localStorage.SetItemAsync("authTokenExpiry", expiry).ConfigureAwait(false);
            }
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        public async Task<string> GetTokenAsync()
        {
            DateTime expiry = await localStorage.GetItemAsync<DateTime>("authTokenExpiry").ConfigureAwait(false);

            if (expiry != default)
            {
                if (expiry > DateTime.Now)
                    return await localStorage.GetItemAsync<string>("authToken").ConfigureAwait(false);
                else
                    await SetTokenAsync(null).ConfigureAwait(false);
            }

            return null;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string token = await GetTokenAsync().ConfigureAwait(false);
            var identity = string.IsNullOrWhiteSpace(token)
                ? new ClaimsIdentity()
                : new ClaimsIdentity(AuthenticationServiceExtentions.ParseClaimsFromJwt(token), "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}
