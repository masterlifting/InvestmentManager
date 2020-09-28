using InvestManager.Entities.Relationship.InterfaceForeignKey;
using System;

namespace InvestManager.Entities.Basic
{
    public interface IBaseBroker : IAccountFK, ICurrencyFK
    {
        long Id { get; set; }
        DateTime DateUpdate { get; set; }
        DateTime DateOperation { get; set; }
    }
}
