using InvestmentManager.Entities.Market;
using InvestmentManager.ReportFinder.Implimentations;
using InvestmentManager.ReportFinder.Interfaces;
using InvestmentManager.Repository;
using InvestmentManager.Service.Implimentations;
using InvestmentManager.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace InvestmentManager.ReportFinder
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region Соединяюсь с БД и работаю с Unit Of Work
            string connectionString = "Server=Apestunov;Database=InvestmentManager;Trusted_Connection=True;MultipleActiveResultSets=true";
            var dbContextOptions = new DbContextOptionsBuilder<InvestmentContext>().UseSqlServer(connectionString).Options;
            var investmentContext = new InvestmentContext(dbContextOptions);
            IUnitOfWorkFactory unitOfWork = new UnitOfWorkFactory(investmentContext);
            #endregion
            #region Регистрирую и конфигурирую сервис зависимостей для нормального управления HttpClient
            var serviceCollection = new ServiceCollection();
            Configure(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            static void Configure(IServiceCollection services)
            {
                services.AddHttpClient<CustomHttpClient>("investing", x =>
                {
                    x.DefaultRequestHeaders.Add("Accept", "application/json,text/javascript,*/*; q=0.01");
                    x.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
                    x.DefaultRequestHeaders.Add("Connection", "keep-alive");
                    x.DefaultRequestHeaders.Add("DNT", "1");
                    x.DefaultRequestHeaders.Add("Origin", "https://ru.investing.com");
                    x.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0");
                });
            }
            #endregion
            IConverterService converterService = new ConverterService();
            IReportService reportService = new ReportService(serviceProvider, unitOfWork, converterService);

            IDictionary<long, Report> lastReports = unitOfWork.Report.GetLastReports();
            int sourceCount = await unitOfWork.ReportSource.GetAll().CountAsync().ConfigureAwait(false);
            Task<List<Report>> newReportsTask = null;
            var reportsToSave = new List<Report>();
            await foreach (var i in unitOfWork.ReportSource.GetAll().AsAsyncEnumerable())
            {
                Console.WriteLine($"\nОсталось проверить {sourceCount--} компаний.");
                Console.WriteLine($"Беру последний отчет.");
                if (lastReports.ContainsKey(i.CompanyId))
                {
                    var lastReportDate = lastReports[i.CompanyId].DateReport;
                    Console.WriteLine($"Проверяю, прошел ли квартал с момента последнего отчета у компании {i.Value}");
                    if (lastReportDate.AddDays(92) > DateTime.Now)
                    {
                        Console.WriteLine($"У компании {i.Value} с момента последнего отчета еще не прошло 3 месяца.");
                        Console.WriteLine($"Дата последнего отчета: {lastReportDate.ToShortDateString()}.");
                        continue;
                    }
                }

                List<Report> foundReports;
                try
                {
                    Console.WriteLine($"Ищу новые отчеты по компании: {i.Value}");
                    newReportsTask = reportService.FindNewReportsAsync(i.CompanyId, i.Key, i.Value);
                    foundReports = await newReportsTask.ConfigureAwait(false);
                    newReportsTask = null;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ошибка. Остановлено на {i.Value}");
                    Console.WriteLine(newReportsTask.Exception.InnerException.Message);
                    Console.ResetColor();
                    newReportsTask = null;
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"По компании {i.Value} добавлено: {foundReports.Count} новых отчетов");
                Console.ResetColor();

                reportsToSave.AddRange(foundReports);
            }

            Console.WriteLine("\nСохраняю все, что нашел...");
            unitOfWork.Report.CreateEntities(reportsToSave);
            await unitOfWork.CompleteAsync().ConfigureAwait(false);

            Console.WriteLine("Press any key to stop process...");
            Console.ReadKey();
        }
    }
    public class CustomHttpClient
    {
        HttpClient HttpClient { get; }
        public CustomHttpClient(HttpClient httpClient) => HttpClient = httpClient;
        public async Task<HttpResponseMessage> GetReportAsync(string query)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, query);
            var response = await HttpClient.SendAsync(request).ConfigureAwait(false);
            Console.WriteLine(response.StatusCode);
            response.EnsureSuccessStatusCode();
            return response;
        }
    }

}
