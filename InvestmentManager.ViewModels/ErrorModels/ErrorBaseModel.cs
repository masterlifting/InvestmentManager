using System.Collections.Generic;

namespace InvestmentManager.ViewModels.ErrorModels
{
    public class ErrorBaseModel
    {
        public bool IsSuccess { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
