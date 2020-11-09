using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.ViewModels.FormEntityModels
{
    public class FormIsinModel
    {
        public long? Id { get; set; }
        public long CompanyId { get; set; }
        [StringLength(50, ErrorMessage = "Name lenght 2..50 symbols", MinimumLength = 2)]
        public string Name { get; set; }
    }
}
