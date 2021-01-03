using InvestmentManager.BrokerService.Interfaces;
using InvestmentManager.BrokerService.Models;
using InvestmentManager.Entities.Basic;
using InvestmentManager.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.BrokerService.Implimentations
{
    public class ReportFilter : IReportFilter
    {
        private readonly InvestmentContext context;
        public ReportFilter(InvestmentContext context) => this.context = context;

        public List<FilterReportModel> GetUniqueLoadedReports(IEnumerable<FilterReportModel> models)
        {
            var monthReportList = new List<FilterReportModel>();
            var dayReportList = new List<FilterReportModel>();
            // Пришедшую коллецию из отчетоа делю на 2 коллекции: Месячные и дневные отчеты, сверяя их по периоду в отчете
            foreach (var i in models)
            {
                if (i.DateBegin != i.DateEnd)
                    monthReportList.Add(i);
                else
                    dayReportList.Add(i);
            }

            //оставляю только уникальные отчеты
            foreach (var i in monthReportList)
            {
                for (int j = 0; j < dayReportList.Count; j++)
                {
                    // Если у месячного и дневного отчета совпадают аккаунты и если даты дневного отчета попадают в период месячного отчета, то удаляем этот дневной отчет
                    if (i.AccountName.Equals(dayReportList[j].AccountName)
                        && dayReportList[j].DateBegin >= i.DateBegin
                        && dayReportList[j].DateBegin <= i.DateEnd)
                    {
                        dayReportList.RemoveAt(j);
                        j--;
                    }
                }
            }
            // объеденяем месячные отчеты и оставшиеся после фильтра дневные
            var result = monthReportList.Union(dayReportList);

            return result.OrderBy(x => x.DateBegin).ToList();
        }
        public async Task<List<T>> GetNewTransactionsAsync<T>(IEnumerable<T> transactions, long accountId, Func<List<T>, T[], List<T>> additionalCheck) where T : class, IBaseBroker
        {
            var result = new List<T>();
            if (transactions is null || !transactions.Any())
                return result;

            /*Получаю выборку из входящей коллекции по этому аккаунту.
             Группирую и сортирую по дате.
             Выбираю первую и последнюю даты опирации для фильтра из базы данных*/
            var incomeTransactions = transactions.Where(x => x.AccountId == accountId).GroupBy(x => x.DateOperation).OrderByDescending(x => x.Key).ToList();
            DateTime firstIncomeDate = incomeTransactions.Last().Key;
            DateTime lastIncomeDate = incomeTransactions.First().Key;

            /*Получаю коллекцию из базы по этому типу транзакции и аккаунту и в периоде дат полученных транзакций
             Группирую и сортирую по дате*/
            var dbTransactions = (await context.Set<T>().Where(x => x.AccountId == accountId && x.DateOperation >= firstIncomeDate && x.DateOperation <= lastIncomeDate).ToArrayAsync())
                .GroupBy(x => x.DateOperation)
                .OrderByDescending(x => x.Key);

            // Если в базе есть что-то, то начинаю алгоритм. Если нет, то добавляю все пришедшие данные по этому аккаунту к результату
            if (dbTransactions.Any())
            {
                foreach (var i in dbTransactions)
                {
                    // получаю дату группы коллекции из БД
                    DateTime dbDateOperation = i.Key;

                    for (int j = 0; j < incomeTransactions.Count; j++)
                    {
                        // получаю дату группы пришедшей коллекции
                        DateTime incomeDateOperation = incomeTransactions[j].Key;

                        // Если даты групп совпадают и колличество операций за эту дату совпадает, то удаляю совпадение из входящей сгруппированой коллекции по этому аккаунту
                        if (incomeDateOperation == dbDateOperation && incomeTransactions[j].Count() == i.Count())
                        {
                            incomeTransactions.RemoveAt(j);
                            j--;
                        }
                    }
                }

                // Если остались еще новые операции за дату, которая уже есть в бд, то проверим их и добавим только новые. Проверенные удалим
                var intersectDateOperations = incomeTransactions.Join(dbTransactions, x => x.Key, y => y.Key, (x, y) => new { FromNew = x, FromDb = y });
                if (intersectDateOperations.Any())
                {
                    List<IGrouping<DateTime, T>> incomeTransactionsToDelete = new();

                    foreach (var operations in intersectDateOperations)
                    {
                        result.AddRange(additionalCheck.Invoke(operations.FromNew.ToList(), operations.FromDb.ToArray()));
                        incomeTransactionsToDelete.Add(operations.FromNew);
                    }

                    for (int i = 0; i < incomeTransactionsToDelete.Count; i++)
                        incomeTransactions.Remove(incomeTransactionsToDelete[i]);
                }

                // Если есть операции, которых нет в бд за эту дату, то добавим их к результату
                if (incomeTransactions.Any())
                {
                    var newOperations = incomeTransactions.Except(dbTransactions);
                    if (newOperations.Any())
                        foreach (var operations in newOperations)
                            result.AddRange(operations);
                }
            }
            else
                result.AddRange(transactions.Where(x => x.AccountId == accountId));

            return result;
        }
    }
}