using System;
using System.ComponentModel.DataAnnotations;

namespace InvestManager.Entities.Basic
{
    public abstract class BaseEntity : IBaseEntity
    {
        protected BaseEntity() => DateUpdate = DateTime.Now;

        [Key]
        public long Id { get; set; }

        public DateTime DateUpdate { get; set; }
    }
}
