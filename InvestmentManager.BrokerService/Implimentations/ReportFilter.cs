using InvestManager.BrokerService.Interfaces;
using InvestManager.BrokerService.Models;
using InvestManager.Entities.Basic;
using InvestManager.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvestManager.BrokerService.Implimentations
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
        public List<T> GetNewTransactions<T>(IEnumerable<T> transactions, long accountId) where T : class, IBaseBroker
        {
            var result = new List<T>();

            // получаю коллекцию из базы по этому типу транзакции и аккаунту
            var dbTransactions = context.Set<T>().AsEnumerable().Where(x => x.AccountId == accountId).GroupBy(x => x.DateOperation).OrderBy(x => x.Key);
            // получаю выборку из входящей коллекции по этому аккаунту
            var incomeTransactions = transactions.Where(x => x.AccountId == accountId).GroupBy(x => x.DateOperation).OrderBy(x => x.Key).ToList();
            // Обе коллекции были сгруппированы и отсортированы по дате операции

            // Если в базе есть что-то, то начинаю алгоритм. Если нет, то добавляю все пришедшие данные по этому аккаунту к результату
            if (dbTransactions.Any())
            {
                foreach (var i in dbTransactions)
                {
                    // получаю дату группы коллекции из БД
                    DateTime dbDateOperation = i.Key.Date;

                    for (int j = 0; j < incomeTransactions.Count; j++)
                    {
                        // получаю дату группы пришедшей коллекции
                        DateTime incomeDateOperation = incomeTransactions[j].Key.Date;

                        // Если даты групп совпадают и колличество операций за эту дату совпадает, то удаляю совпадение из входящей сгруппированой коллекции по этому аккаунту
                        if (incomeDateOperation.Equals(dbDateOperation) && incomeTransactions[j].Count().Equals(i.Count()))
                        {
                            incomeTransactions.RemoveAt(j);
                            j--;
                        }
                    }
                }

                // То, что осталось после фильтра добавляю к результату
                foreach (var i in incomeTransactions)
                    result.AddRange(i.Select(x => x));
            }
            else
                result.AddRange(transactions.Where(x => x.AccountId == accountId));

            return result;
        }
    }
}
