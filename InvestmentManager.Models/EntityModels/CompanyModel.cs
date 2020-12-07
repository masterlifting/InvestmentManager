using InvestmentManager.Entities.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Models.EntityModels
{
    public class CompanyModel : ShortView
    {
        [Required(ErrorMessage = "Name required!")]
        [StringLength(50, ErrorMessage = "Name lenght 2..50 symbols", MinimumLength = 2)]
        public override string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateSplit { get; set; }
        [Zero(ErrorMessage = "Select this!")]
        public long IndustryId { get; set; }
        [Zero(ErrorMessage = "Select this!")]
        public long SectorId { get; set; }
    }
}
