using System;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Basic
{
    public abstract class BaseEntity : IBaseEntity
    {
        protected BaseEntity() => DateUpdate = DateTime.Now;

        [Key]
        public long Id { get; set; }

        public DateTime DateUpdate { get; set; }
    }
}
