using System.Collections.Generic;

namespace InvestmentManager.ViewModels.ErrorModels
{
    public class ErrorBaseModel
    {
        public bool IsSuccess { get; set; } = false;
        public IEnumerable<string> Errors { get; set; }
    }
}
