using System.Collections.Generic;

namespace InvestmentManager.Models
{
    public class BaseListViewModel<T> where T : class
    {
        public string ResultInfo { get; set; }
        public List<T> ResultContents { get; set; }

    }
}
