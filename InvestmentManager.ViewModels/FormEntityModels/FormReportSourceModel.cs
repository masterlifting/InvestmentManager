using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.ViewModels.FormEntityModels
{
    public class FormReportSourceModel
    {
        public long? Id { get; set; }
        public long CompanyId { get; set; }
        [Required(ErrorMessage = "This is required field!")]
        public string Key { get; set; }
        [Required(ErrorMessage = "This is required field!")]
        public string Value { get; set; }
    }
}
