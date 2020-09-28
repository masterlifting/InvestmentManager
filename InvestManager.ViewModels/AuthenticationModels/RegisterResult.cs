using System.Collections.Generic;

namespace InvestManager.ViewModels.AuthenticationModels
{
    public class RegisterResult
    {
        public bool Successful { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
