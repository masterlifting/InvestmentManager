using System;

namespace InvestmentManager.Models.Security
{
    public class LoginResult : BaseActionResult
    {
        public string Token { get; set; }
        public DateTime Expiry { get; set; }
    }
}
