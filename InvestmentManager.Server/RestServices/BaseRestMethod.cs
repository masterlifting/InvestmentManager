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

        public async Task<BaseActionResult> BasePostAsync<TEntity, TModel>(ModelStateDictionary modelState, TEntity result, TModel model, Func<TModel, Task<bool>> customValidator = null) where TEntity : class where TModel : class
        {
            if (!modelState.IsValid)
                return new BaseActionResult { IsSuccess = false, Info = modelInvalid };

            if (customValidator is not null)
            {
                try
                {
                    if (!await customValidator.Invoke(model))
                        return new BaseActionResult { IsSuccess = false, Info = $"Impossible action." };
                }
                catch
                {
                    return new BaseActionResult { IsSuccess = false, Info = "Validation error." };
                }
            }

            await context.Set<TEntity>().AddAsync(result);

            try
            {
                await context.SaveChangesAsync();
            }
            catch
            {
                try
                {
                    var tableName = context.Model.FindEntityType(typeof(TEntity)).GetTableName();
                    var idlimit = context.Set<TEntity>().AsEnumerable().Max(x => x.GetType().GetProperty("Id").GetValue(x));
                    long nextId = (long)idlimit + 1;
                    context.Database.ExecuteSqlRaw($"ALTER SEQUENCE \"{tableName}_Id_seq\" RESTART WITH {nextId}");

                    await context.SaveChangesAsync();
                }
                catch
                {
                    return new BaseActionResult { IsSuccess = false, Info = savingError };
                }
            }

            return new BaseActionResult { IsSuccess = true, Info = $"{typeof(TEntity).Name} saved.", ResultId = (long)result.GetType().GetProperty("Id").GetValue(result) };
        }
        public async Task<BaseActionResult> BasePutAsync<TEntity>(ModelStateDictionary modelState, long id, Action<TEntity> update) where TEntity : class
        {
            if (!modelState.IsValid)
                return new BaseActionResult { IsSuccess = false, Info = modelInvalid };

            var entity = await context.Set<TEntity>().FindAsync(id);

            if (entity is null)
                return new BaseActionResult { IsSuccess = false, Info = modelIsNull };

            if (update is null)
                return new BaseActionResult { IsSuccess = false, Info = "Update action out." };
            else
                update.Invoke(entity);

            try
            {
                await context.SaveChangesAsync();
            }
            catch
            {
                return new BaseActionResult { IsSuccess = false, Info = editingError };
            }

            return new BaseActionResult { IsSuccess = true, Info = $"{typeof(TEntity).Name} edited.", ResultId = id };
        }
        public async Task<BaseActionResult> BaseDeleteAsync<TEntity>(long id) where TEntity : class
        {
            var entity = await context.Set<TEntity>().FindAsync(id);

            if (entity is null)
                return new BaseActionResult { IsSuccess = false, Info = modelIsNull };

            try
            {
                context.Set<TEntity>().Remove(entity);
                await context.SaveChangesAsync();
            }
            catch
            {
                return new BaseActionResult { IsSuccess = false, Info = deletingError };
            }

            return new BaseActionResult { IsSuccess = true, Info = $"{typeof(TEntity).Name} deleted.", ResultId = id };
        }
    }
}
