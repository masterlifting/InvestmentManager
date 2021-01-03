using InvestmentManager.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace InvestmentManager.Server.RestServices
{
    public interface IBaseRestMethod
    {
        Task<BaseActionResult> BasePostAsync<TEntity, TModel>(ModelStateDictionary modelState, TEntity result, TModel model, Func<TModel, Task<bool>> func = null) where TEntity : class where TModel : class;
        Task<BaseActionResult> BasePutAsync<TEntity>(ModelStateDictionary modelState, long id, Action<TEntity> update) where TEntity : class;
        Task<BaseActionResult> BaseDeleteAsync<TEntity>(long id) where TEntity : class;
    }
}
