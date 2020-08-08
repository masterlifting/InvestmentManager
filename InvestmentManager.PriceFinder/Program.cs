using InvestmentManager.Entities.Market;
using InvestmentManager.Repository;
using InvestmentManager.StockPriceFinder.Implimentations;
using InvestmentManager.StockPriceFinder.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace InvestmentManager.StockPriceFinder
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var newPricies = new List<Price>();
            #region Соединяюсь с БД и работаю с Unit Of Work
            string connectionString = "Server=Apestunov;Database=InvestmentManager;Trusted_Connection=True;MultipleActiveResultSets=true";
            var dbContextOptions = new DbContextOptionsBuilder<InvestmentContext>().UseSqlServer(connectionString).Options;
            var investmentContext = new InvestmentContext(dbContextOptions);
            IUnitOfWorkFactory unitOfWork = new UnitOfWorkFactory(investmentContext);
            #endregion
            #region Регистрирую и конфигурирую сервис зависимостей для нормального управления HttpClient
            var serviceCollection = new ServiceCollection();
            Configure(serviceCollection);
            var httpService = serviceCollection.BuildServiceProvider();
            static void Configure(IServiceCollection services) => services.AddHttpClient<CustomHttpClient>();
            #endregion

            IPriceService priceService = new PriceService(httpService, unitOfWork);
            var exchanges = unitOfWork.Exchange.GetAll();
            var tickers = unitOfWork.Ticker.GetUniqueTikersForPrice();
            var priceConfigure = tickers.Join(exchanges, x => x.ExchangeId, y => y.Id, (x, y) => new { TickerId = x.Id, Ticker = x.Name, y.ProviderName, y.ProviderUri });

            int count = priceConfigure.Count();

            foreach (var i in priceConfigure)
            {
                try
                {
                    var newPrice = await priceService.GetPriceListAsync(i.ProviderName, i.TickerId, i.Ticker, i.ProviderUri).ConfigureAwait(false);
                    newPricies.AddRange(newPrice);
                    Console.WriteLine($"Цены по {i.Ticker} загружены в количестве: {newPrice.Count} шт. Осталось компаний {--count}.");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"На компании {i.Ticker} произошла ошибка {ex.Message}. Осталось компаний {--count}.");
                }
            }

            unitOfWork.Price.CreateEntities(newPricies);
            await unitOfWork.CompleteAsync().ConfigureAwait(false);

            Console.WriteLine("Press any key to stop process...");
            Console.ReadKey();
        }
    }
    public class CustomHttpClient
    {
        HttpClient HttpClient { get; }
        public CustomHttpClient(HttpClient httpClient) => HttpClient = httpClient;
        // Gets the list of services.
        public async Task<HttpResponseMessage> GetPriceAsync(string query)
        {
            await Task.Delay(500).ConfigureAwait(false);

            var request = new HttpRequestMessage(HttpMethod.Get, query);
            var response = await HttpClient.SendAsync(request).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Большое колличество запросов. Ждем 10 сек");
                Console.ResetColor();

                await Task.Delay(10000).ConfigureAwait(false);
                request = new HttpRequestMessage(HttpMethod.Get, query);
                response = await HttpClient.SendAsync(request).ConfigureAwait(false);
            }

            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}
