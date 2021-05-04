using System;

namespace InvestmentManager.Models.Security
{
    public class AuthResult : BaseActionResult
    {
        public string Token { get; set; }
        public DateTime Expiry { get; set; }
    }
}
