using InvestmentManager.BrokerService.Models;
using InvestmentManager.Entities.Basic;
using System.Collections.Generic;

namespace InvestmentManager.BrokerService.Interfaces
{
    public interface IReportFilter
    {
        /// <summary>
        /// Из загруженных excel отчетов оставляем отчеты только за уникальные даты. Если приши месячный и дневной отчеты и при этом месячный уже содержит дневной, то дневной отчет удаляется.
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        List<FilterReportModel> GetUniqueLoadedReports(IEnumerable<FilterReportModel> models);
        /// <summary>
        /// Из входящей коллекции транзакций оставляет только те операции, которых нет в базе данных
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transactions"></param>
        /// <returns></returns>
        List<T> GetNewTransactions<T>(IEnumerable<T> transactions, long accountId) where T : class, IBaseBroker;
    }
}
