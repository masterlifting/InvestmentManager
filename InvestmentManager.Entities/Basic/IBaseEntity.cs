using System;

namespace InvestmentManager.Entities.Basic
{
    public interface IBaseEntity
    {
        long Id { get; set; }
        DateTime DateUpdate { get; set; }
    }
}
