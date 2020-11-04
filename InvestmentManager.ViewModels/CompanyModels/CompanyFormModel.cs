using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.ViewModels.CompanyModels
{
    public class CompanyFormModel
    {
        public long? Id { get; set; }

        [Required(ErrorMessage = "Name required!")]
        [StringLength(50, ErrorMessage = "Name lenght 2..50 symbols", MinimumLength = 2)]
        public string Name { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateSplit { get; set; }
        [StringLength(2, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string IndustryId { get; set; }
        [StringLength(2, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string SectorId { get; set; }
        [ValidateComplexType]
        public ReportSourceModel ReportSource { get; set; }
        [ValidateComplexType]
        public List<TickerModel> Tickers { get; set; }
        [ValidateComplexType]
        public List<IsinModel> Isins { get; set; }
    }
    public class TickerModel
    {
        public long? Id { get; set; }

        [Required(ErrorMessage = "Ticker required!")]
        [StringLength(10, ErrorMessage = "Name lenght 1..10 symbols", MinimumLength = 1)]
        public string Name { get; set; }
        [StringLength(2, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string ExcangeId { get; set; }
        [StringLength(2, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string LotId { get; set; }
    }
    public class IsinModel 
    {
        public long? Id { get; set; }
        [StringLength(50, ErrorMessage = "Name lenght 2..50 symbols", MinimumLength = 2)]
        public string Name { get; set; }
    }
    public class ReportSourceModel
    {
        public long? Id { get; set; }
        [Required(ErrorMessage = "This is required field!")]
        public string Key { get; set; }
        [Required(ErrorMessage = "This is required field!")]
        public string Value { get; set; }
    }
    public class CompanyFormModelData
    {
        public List<ViewModelBase> Exchanges { get; set; }
        public List<ViewModelBase> Lots { get; set; }
        public List<ViewModelBase> Sectors { get; set; }
        public List<ViewModelBase> Industries { get; set; }
    }
}
