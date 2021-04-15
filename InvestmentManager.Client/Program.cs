using Blazored.LocalStorage;
using InvestmentManager.Client.Services.AuthenticationConfiguration;
using InvestmentManager.Client.Services.HttpService;
using InvestmentManager.Client.Services.NotificationService;
using InvestmentManager.Services.Implimentations;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace InvestmentManager.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped<CustomNotification>();
            builder.Services.AddBlazoredLocalStorage();
            
            builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<CustomHttpClient>();
            builder.Services.AddScoped(typeof(APIService<>));
            builder.Services.AddScoped<API>();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddCustomAuthenticationStateProvider();
            //builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            
            builder.Services.AddSingleton<ICatalogService, CatalogService>();
            

            await builder.Build().RunAsync();
        }
    }
}
