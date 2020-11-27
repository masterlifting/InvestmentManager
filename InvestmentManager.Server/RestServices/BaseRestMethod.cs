using static InvestmentManager.Models.ErrorMessages;
using InvestmentManager.Models;
using InvestmentManager.Repository;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InvestmentManager.Server.RestServices
{
    public class BaseRestMethod : IBaseRestMethod
    {
        private readonly InvestmentContext context;
        public BaseRestMethod(InvestmentContext context) => this.context = context;

        public async Task<BaseResult> BasePostAsync<TEntity, TModel>(ModelStateDictionary modelState, TEntity result, TModel model, Func<TModel, bool> func = null) where TEntity : class where TModel : class
        {
            if (!modelState.IsValid)
                return new BaseResult { IsSuccess = false, Info = modelInvalid };

            bool isContains = false;

            if (func != null)
            {
                try
                {
                    isContains = func.Invoke(model);
                }
                catch
                {
                    return new BaseResult { IsSuccess = false, Info = "Contains function error" };
                }
            }

            if (isContains)
                return new BaseResult { IsSuccess = true, Info = $"This {typeof(TEntity).Name} allready!" };

            await context.Set<TEntity>().AddAsync(result).ConfigureAwait(false);

            try
            {
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch
            {
                try
                {
                    var tableName = context.Model.FindEntityType(typeof(TEntity)).GetTableName();
                    var idlimit = context.Set<TEntity>().AsEnumerable().Max(x => x.GetType().GetProperty("Id").GetValue(x));
                    long nextId = (long)idlimit + 1;
                    context.Database.ExecuteSqlRaw($"ALTER SEQUENCE \"{tableName}_Id_seq\" RESTART WITH {nextId}");

                     await context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch
                {
                    return new BaseResult { IsSuccess = false, Info = savingError };
                }
            }

            return new BaseResult { IsSuccess = true, Info = $"{typeof(TEntity).Name} saved.", ResultId = (long)result.GetType().GetProperty("Id").GetValue(result) };
        }
        public async Task<BaseResult> BasePutAsync<TEntity>(ModelStateDictionary modelState, long id, Action<TEntity> update) where TEntity : class
        {
            if (!modelState.IsValid)
                return new BaseResult { IsSuccess = false, Info = modelInvalid };

            var entity = await context.Set<TEntity>().FindAsync(id).ConfigureAwait(false);

            if (entity is null)
                return new BaseResult { IsSuccess = false, Info = modelIsNull };

            update.Invoke(entity);

            try
            {
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch
            {
                return new BaseResult { IsSuccess = false, Info = editingError };
            }

            return new BaseResult { IsSuccess = true, Info = $"{typeof(TEntity).Name} edited.", ResultId = id };
        }
        public async Task<BaseResult> BaseDeleteAsync<TEntity>(long id) where TEntity : class
        {
            var entity = await context.Set<TEntity>().FindAsync(id).ConfigureAwait(false);

            if (entity is null)
                return new BaseResult { IsSuccess = false, Info = modelIsNull };

            try
            {
                context.Set<TEntity>().Remove(entity);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch
            {
                return new BaseResult { IsSuccess = false, Info = deletingError };
            }

            return new BaseResult { IsSuccess = true, Info = $"{typeof(TEntity).Name} deleted.", ResultId = id };
        }
    }
}
