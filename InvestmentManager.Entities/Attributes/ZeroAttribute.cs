using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Attributes
{
    public class ZeroAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string convertedValue = value.ToString();

            bool result = decimal.TryParse(convertedValue, out decimal decimalResult);

            if (result && decimalResult != default)
                return true;
            return false;
        }
    }
}
