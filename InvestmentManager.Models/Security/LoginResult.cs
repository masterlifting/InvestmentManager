using System;

namespace InvestmentManager.Models.Security
{
    public class LoginResult : BaseResult
    {
        public string Token { get; set; }
        public DateTime Expiry { get; set; }
    }
}
