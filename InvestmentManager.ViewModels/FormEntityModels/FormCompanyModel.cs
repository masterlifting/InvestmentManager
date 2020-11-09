using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.ViewModels.FormEntityModels
{
    public class FormCompanyModel
    {
        public long? Id { get; set; }

        [Required(ErrorMessage = "Name required!")]
        [StringLength(50, ErrorMessage = "Name lenght 2..50 symbols", MinimumLength = 2)]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateSplit { get; set; }

        public List<ViewModelBase> Industries { get; set; }
        [StringLength(2, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string IndustryId { get; set; }

        public List<ViewModelBase> Sectors { get; set; }
        [StringLength(2, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string SectorId { get; set; }
    }

}
