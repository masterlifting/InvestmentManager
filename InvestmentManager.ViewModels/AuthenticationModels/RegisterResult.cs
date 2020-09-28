using System.Collections.Generic;

namespace InvestmentManager.ViewModels.AuthenticationModels
{
    public class RegisterResult
    {
        public bool Successful { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
