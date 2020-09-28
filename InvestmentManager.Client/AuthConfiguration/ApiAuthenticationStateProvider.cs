using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace InvestmentManager.Client.AuthConfiguration
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient httpClient;
        private readonly ILocalStorageService localStorage;

        public ApiAuthenticationStateProvider(
            HttpClient httpClient
            , ILocalStorageService localStorage)
        {
            this.httpClient = httpClient;
            this.localStorage = localStorage;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var savedToken = await localStorage.GetItemAsync<string>("authToken").ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(savedToken))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(savedToken), "jwt")));

            static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
            {
                var claims = new List<Claim>();
                string payload = jwt.Split('.')[1];
                byte[] jsonBytes = ParseBase64WithoutPadding(payload);
                var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

                keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

                if (roles != null)
                {
                    if (roles.ToString().Trim().StartsWith("["))
                    {
                        var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());

                        foreach (var parsedRole in parsedRoles)
                            claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                    }
                    else
                        claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));

                    keyValuePairs.Remove(ClaimTypes.Role);
                }

                claims.AddRange(keyValuePairs.Select(x => new Claim(x.Key, x.Value.ToString())));

                return claims;

                static byte[] ParseBase64WithoutPadding(string base64)
                {
                    switch (base64.Length % 4)
                    {
                        case 2: base64 += "=="; break;
                        case 3: base64 += "="; break;
                    }
                    return Convert.FromBase64String(base64);
                }
            }
        }
        public void MarkUserAsAuthenticated(string email)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, email) }, "apiauth"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }
        public void MarkUserAsLogout()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
