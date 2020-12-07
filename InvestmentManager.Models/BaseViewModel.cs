namespace InvestmentManager.Models
{
    public class BaseViewModel<T> where T : class
    {
        public string ResultInfo { get; set; }
        public T ResultContent { get; set; }
    }
}
