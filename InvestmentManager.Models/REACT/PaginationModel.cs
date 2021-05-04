namespace InvestmentManager.Models
{
    public class PaginationModel<T> where T : class
    {
        public T[] Items { get; set; }
        public int TotalCount { get; set; }
    }
}
