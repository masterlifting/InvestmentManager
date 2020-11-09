using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.ViewModels.FormEntityModels
{
    public class FormTickerModel
    {
        public long? Id { get; set; }
        public long CompanyId { get; set; }

        [Required(ErrorMessage = "Ticker required!")]
        [StringLength(10, ErrorMessage = "Name lenght 1..10 symbols", MinimumLength = 1)]
        public string Name { get; set; }

        public List<ViewModelBase> Exchanges { get; set; }
        [StringLength(2, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string ExcangeId { get; set; }

        public List<ViewModelBase> Lots { get; set; }
        [StringLength(2, ErrorMessage = DefaultData.errorLenght), Required(ErrorMessage = DefaultData.errorRequired)]
        public string LotId { get; set; }
    }
}
