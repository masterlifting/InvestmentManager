﻿using InvestmentManager.Repository;
using InvestmentManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using InvestmentManager.Entities.Market;
using System;
using InvestmentManager.Server.RestServices;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Services.Interfaces;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IBaseRestMethod restMethod;
        private readonly ICatalogService catalogService;

        public CompaniesController(
            IUnitOfWorkFactory unitOfWork
            , IBaseRestMethod restMethod
            , ICatalogService catalogService)
        {
            this.unitOfWork = unitOfWork;
            this.restMethod = restMethod;
            this.catalogService = catalogService;
        }

        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await unitOfWork.Company.GetAll().OrderBy(x => x.Name).Select(x => new ShortView { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false));
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var company = await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false);
            return company is null ? NoContent() : Ok(new CompanyModel
            {
                Id = company.Id,
                Name = company.Name,
                DateSplit = company.DateSplit,
                IndustryId = company.IndustryId,
                SectorId = company.SectorId
            });
        }
        [HttpGet("{id}/summary/")]
        public async Task<IActionResult> GetSummary(long id)
        {
            var company = await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false);
            long? currencyId = company?.Tickers?.FirstOrDefault()?.Prices?.FirstOrDefault()?.CurrencyId;
            return company is null ? NoContent() : Ok(new SummaryAdditional
            {
                IndustryName = company.Industry.Name,
                SectorName = company.Sector.Name,
                CurrencyType = currencyId.HasValue ? catalogService.GetCurrencyName(currencyId.Value) : "No currency data."
            });
        }

        [HttpGet("bypagination/{value}")]
        public async Task<IActionResult> GetPagination(int value = 1)
        {
            int pageSize = 10;
            var companies = unitOfWork.Company.GetAll().OrderBy(x => x.Name);
            var items = await companies
                .Skip((value - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ShortView { Id = x.Id, Name = x.Name })
                .ToListAsync().ConfigureAwait(false);

            var paginationResult = new PaginationViewModel<ShortView>();
            paginationResult.Pagination.SetPagination(await companies.CountAsync().ConfigureAwait(false), value, pageSize);
            paginationResult.Items = items;

            return Ok(paginationResult);
        }

        [HttpPost, Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Post(CompanyModel model)
        {
            var entity = new Company
            {
                Name = model.Name,
                DateSplit = model.DateSplit,
                IndustryId = model.IndustryId,
                SectorId = model.SectorId
            };
            bool CompanyContains(CompanyModel model) => unitOfWork.Company.GetAll()
                .Select(x => x.Name)
                .ToList()
                .Where(x => x.Equals(model.Name, StringComparison.OrdinalIgnoreCase))
                .Any();

            var result = await restMethod.BasePostAsync(ModelState, entity, model, CompanyContains).ConfigureAwait(false);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Put(long id, CompanyModel model)
        {
            void UpdateCompany(Company company)
            {
                company.DateSplit = model.DateSplit;
                company.DateUpdate = DateTime.Now;
                company.IndustryId = model.IndustryId;
                company.Name = model.Name;
                company.SectorId = model.SectorId;
            }

            var result = await restMethod.BasePutAsync<Company>(ModelState, id, UpdateCompany);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
