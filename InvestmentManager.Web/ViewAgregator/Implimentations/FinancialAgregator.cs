using InvestmentManager.Repository;
using InvestmentManager.Service.Interfaces;
using InvestmentManager.Web.Models.FinancialModels;
using InvestmentManager.Web.ViewAgregator.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Web.ViewAgregator.Implimentations
{
    public class FinancialAgregator : IFinancialAgregator
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IConverterService converter;

        public FinancialAgregator(IUnitOfWorkFactory unitOfWork, IConverterService converter)
        {
            this.unitOfWork = unitOfWork;
            this.converter = converter;
        }

        public async Task<List<PriceModel>> GetPricesAsync(long? id)
        {
            var listModel = new List<PriceModel>();

            if (id.HasValue)
            {
                var oneModel = new PriceModel();

                var prices = await unitOfWork.Price.GetCustomPricesAsync(id.Value, 12, OrderType.OrderBy).ConfigureAwait(false);
                var company = await unitOfWork.Company.FindByIdAsync(id.Value).ConfigureAwait(false);
                var currency = await unitOfWork.Currency.FindByIdAsync(prices.First().CurrencyId).ConfigureAwait(false);
                if (prices.Any())
                {
                    oneModel.CurrentPrice = prices.Last().Value;
                    oneModel.DateUpdate = prices.Last().BidDate.ToShortDateString();
                    oneModel.MinPrice = prices.Min(x => x.Value);
                    oneModel.MaxPrice = prices.Max(x => x.Value);
                    oneModel.CurrencyType = currency.Name;
                }

                if (company != null)
                {
                    oneModel.CompanyId = company.Id;
                    oneModel.CompanyName = company.Name;
                }

                oneModel.RecommendationPrice = 0;

                listModel.Add(oneModel);
            }
            else
            {
                var companies = await unitOfWork.Company.GetAll().ToListAsync().ConfigureAwait(false);
                var currencyTypes = unitOfWork.Currency.GetAll();
                var prices = unitOfWork.Price.GetGroupedPrices(12, OrderType.OrderBy);

                foreach (var i in companies.Join(prices, x => x.Id, y => y.Key, (x, y) => new
                {
                    CompanyId = x.Id,
                    CompanyName = x.Name,
                    CurrentPrice = y.Value.Last().Value,
                    DateUpdate = y.Value.Last().BidDate,
                    MinPrice = y.Value.Min(z => z.Value),
                    MaxPrice = y.Value.Max(z => z.Value),
                    y.Value.First().CurrencyId
                })
                .Join(currencyTypes, x => x.CurrencyId, y => y.Id, (x, y) => new
                {
                    x.CompanyId,
                    x.CurrentPrice,
                    x.DateUpdate,
                    x.MinPrice,
                    x.MaxPrice,
                    x.CompanyName,
                    Currency = y.Name
                }))
                {
                    listModel.Add(new PriceModel
                    {
                        CompanyId = i.CompanyId,
                        CompanyName = i.CompanyName,
                        CurrentPrice = i.CurrentPrice,
                        DateUpdate = i.DateUpdate.ToShortDateString(),
                        MinPrice = i.MinPrice,
                        MaxPrice = i.MaxPrice,
                        CurrencyType = i.Currency,
                        RecommendationPrice = 0
                    });
                }
            }

            return listModel;
        }
        public async Task<ReportModel> GetReportsAsync(long? id)
        {
            var model = new ReportModel();
            var headers = new List<ReportHeadModel>();

            if (id.HasValue)
            {
                var reports = unitOfWork.Report.GetAll().Where(x => x.CompanyId == id);
                var company = await unitOfWork.Company.FindByIdAsync(id.Value).ConfigureAwait(false);
                var dateReport = unitOfWork.Report.GetLastDateReport(id.Value);

                if (!reports.Any() || company is null)
                    return model;

                headers.Add(new ReportHeadModel
                {
                    CompanyId = id.Value,
                    CompanyName = company.Name,
                    TotalCount = reports.Count(),
                    LastYear = dateReport.Year,
                    LastQuarter = converter.ConvertToQuarter(dateReport.Month)
                });
            }
            else
            {
                var companies = await unitOfWork.Company.GetAll().ToListAsync().ConfigureAwait(false);
                var reports = unitOfWork.Report.GetAll();
                var dateReports = unitOfWork.Report.GetLastDateReports();

                if (!reports.Any() || !companies.Any())
                    return model;

                foreach (var i in companies.GroupJoin(reports, x => x.Id, y => y.CompanyId, (x, y) => new
                { CompanyId = x.Id, CompanyName = x.Name, Reports = y })
                                            .Join(dateReports, x => x.CompanyId, y => y.Key, (x, y) => new
                                            { x.CompanyId, x.CompanyName, x.Reports, LastDateReport = y.Value }))
                {
                    headers.Add(new ReportHeadModel
                    {
                        CompanyId = i.CompanyId,
                        CompanyName = i.CompanyName,
                        TotalCount = i.Reports.Count(),
                        LastYear = i.LastDateReport.Year,
                        LastQuarter = converter.ConvertToQuarter(i.LastDateReport.Month)
                    });
                }
            }
            model.Headers = headers;
            return model;
        }
        public List<ReportBodyModel> BuildReportBody(long? companyId)
        {
            var result = new List<ReportBodyModel>();
            
            if(companyId.HasValue)
            {
                foreach (var i in unitOfWork.Report.GetAll().Where(x => x.CompanyId == companyId.Value).OrderByDescending(x => x.DateReport))
                {
                    result.Add(new ReportBodyModel
                    {
                        Year = i.DateReport.Year,
                        Quarter = converter.ConvertToQuarter(i.DateReport.Month),
                        Assets = i.Assets,
                        CashFlow = i.CashFlow,
                        Dividends = i.Dividends,
                        GrossProfit = i.GrossProfit,
                        LongTermDebt = i.LongTermDebt,
                        NetProfit = i.NetProfit,
                        Obligations = i.Obligations,
                        Revenue = i.Revenue,
                        ShareCapital = i.ShareCapital,
                        StockVolume = i.StockVolume,
                        Turnover = i.Turnover
                    });
                }
            }

            return result;
        }
    }
}
