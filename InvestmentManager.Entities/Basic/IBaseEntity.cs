using System;

namespace InvestManager.Entities.Basic
{
    public interface IBaseEntity
    {
        long Id { get; set; }
        DateTime DateUpdate { get; set; }
    }
}
