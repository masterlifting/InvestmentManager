using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.ViewModels.SecurityModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Введите адрес почты")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Некорректный email")]
        [StringLength(40, ErrorMessage = "Email должен иметь от {2} до {1} символов", MinimumLength = 6)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password, ErrorMessage = "Придумайте пароль посложнее")]
        [StringLength(30, ErrorMessage = "Пароль должен иметь от {2} до {1} символов", MinimumLength = 10)]
        public string Password { get; set; }
    }
}
