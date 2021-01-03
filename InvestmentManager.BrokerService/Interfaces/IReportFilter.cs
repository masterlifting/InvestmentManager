using InvestmentManager.BrokerService.Models;
using InvestmentManager.Entities.Basic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.BrokerService.Interfaces
{
    public interface IReportFilter
    {
        /// <summary>
        /// Из загруженных excel отчетов оставляем отчеты только за уникальные даты. Если приши месячный и дневной отчеты и при этом месячный уже содержит дневной, то дневной отчет удаляется.
        /// </summary>
        /// <param name="models">Загруженные модели отчетов</param>
        /// <returns>Уникальные отчеты за даты отчетов</returns>
        List<FilterReportModel> GetUniqueLoadedReports(IEnumerable<FilterReportModel> models);
        /// <summary>
        /// Из входящей коллекции транзакций оставляет только те операции, которых нет в базе данных
        /// </summary>
        /// <typeparam name="T">Тип операции брокерского отчета</typeparam>
        /// <param name="transactions">Коллекция операций этого типа</param>
        /// <param name="additionalCheck">Дополнительная проверка новых операций</param>
        /// <returns>Только новые операции, которых еще нет в базе по этому аккаунту</returns>
        Task<List<T>> GetNewTransactionsAsync<T>(IEnumerable<T> transactions, long accountId, Func<List<T>, T[], List<T>> additionalCheck) where T : class, IBaseBroker;
    }
}
