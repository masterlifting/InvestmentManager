using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Models.EntityModels
{
    public class ComissionTypeModel
    {
        [Required(ErrorMessage = "Type required!")]
        [StringLength(500, ErrorMessage = "Name lenght 2..500 symbols", MinimumLength = 2)]
        public string Name { get; set; }
    }
}
