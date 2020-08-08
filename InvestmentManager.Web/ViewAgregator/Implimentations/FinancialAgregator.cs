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
        private readonly IConverterService converterService;

        public FinancialAgregator(IUnitOfWorkFactory unitOfWork, IConverterService converterService)
        {
            this.unitOfWork = unitOfWork;
            this.converterService = converterService;
        }

        public async Task<List<PriceComponentModel>> GetPricesComponentAsync(long id)
        {
            var listModel = new List<PriceComponentModel>();

            if (id != default)
            {
                var oneModel = new PriceComponentModel();

                var prices = await unitOfWork.Price.GetSortedPricesByDateAsync(id, OrderType.OrderByDesc).ConfigureAwait(false);
                var company = await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false);

                if (prices.Any())
                {
                    oneModel.CurrentPrice = prices.LastOrDefault().Value;
                    oneModel.DateUpdate = prices.LastOrDefault().BidDate.Date.ToShortDateString();
                    oneModel.MinPrice = prices.Min(x => x.Value);
                    oneModel.MaxPrice = prices.Max(x => x.Value);
                    oneModel.CurrencyType = (await unitOfWork.Currency.FindByIdAsync(prices.First().CurrencyId).ConfigureAwait(false)).Name;
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
                var prices = unitOfWork.Price.GetGroupedSortedDescPrices();

                foreach (var i in companies.Join(prices, x => x.Id, y => y.Key, (x, y) => new
                {
                    CompanyId = x.Id,
                    CompanyName = x.Name,
                    CurrentPrice = y.Value.LastOrDefault().Value,
                    DateUpdate = y.Value.LastOrDefault().BidDate.Date,
                    MinPrice = y.Value.Min(z => z.Value),
                    MaxPrice = y.Value.Max(z => z.Value),
                    y.Value.FirstOrDefault().CurrencyId
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
                    listModel.Add(new PriceComponentModel
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
        public async Task<List<ReportComponentModel>> GetReportsComponentAsync(long id)
        {
            var model = new List<ReportComponentModel>();

            if (id != default)
            {
                var reports = unitOfWork.Report.GetAll().Where(x => x.CompanyId == id);
                var company = await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false);

                if (!reports.Any() || company is null)
                    return model;

                var tempModel = new ReportComponentModel(null)
                {
                    CompanyId = id,
                    CompanyName = company.Name,
                    TotalCount = reports.Count(),
                    LastYear = reports.Select(x => x.DateReport.Year).OrderBy(x => x).Last()
                };

                tempModel.LastQuarter = converterService.GetConvertedMonthInQuarter
                    (
                        reports.Where(x => x.DateReport.Year == tempModel.LastYear)
                               .OrderBy(x => x.DateReport.Month)
                               .Select(x => x.DateReport.Month)
                               .Last());

                model.Add(tempModel);
            }
            else
            {
                var reports = unitOfWork.Report.GetAll();
                var companies = await unitOfWork.Company.GetAll().ToListAsync().ConfigureAwait(false);

                if (!reports.Any() || !companies.Any())
                    return model;

                foreach (var i in companies.GroupJoin(reports, x => x.Id, y => y.CompanyId, (x, y) => new
                {
                    CompanyId = x.Id,
                    CompanyName = x.Name,
                    Reports = y
                }))
                {
                    var tempModel = new ReportComponentModel(null)
                    {
                        CompanyId = i.CompanyId,
                        CompanyName = i.CompanyName,
                    };

                    if (i.Reports.Any())
                    {
                        tempModel.TotalCount = i.Reports.Count();
                        tempModel.LastYear = i.Reports.Select(z => z.DateReport.Year).OrderBy(z => z).Last();
                        tempModel.LastQuarter = converterService.GetConvertedMonthInQuarter
                        (
                            i.Reports.Where(x => x.DateReport.Year == tempModel.LastYear)
                                   .OrderBy(x => x.DateReport.Month)
                                   .Select(x => x.DateReport.Month)
                                   .Last()
                        );
                    }
                    else
                    {
                        tempModel.TotalCount = 0;
                        tempModel.LastYear = 0;
                        tempModel.LastQuarter = 0;
                    }

                    model.Add(tempModel);
                }
            }
            return model;
        }
    }
}
