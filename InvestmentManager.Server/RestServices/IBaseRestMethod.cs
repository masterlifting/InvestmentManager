using InvestmentManager.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace InvestmentManager.Server.RestServices
{
    public interface IBaseRestMethod
    {
        Task<BaseResult> BasePostAsync<TEntity, TModel>(ModelStateDictionary modelState, TEntity result, TModel model, Func<TModel, bool> func = null) where TEntity : class where TModel : class;
        Task<BaseResult> BasePutAsync<TEntity>(ModelStateDictionary modelState, long id, Action<TEntity> update) where TEntity : class;
        Task<BaseResult> BaseDeleteAsync<TEntity>(long id) where TEntity : class;
    }
}
