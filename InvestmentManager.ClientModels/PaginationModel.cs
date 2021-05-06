namespace InvestmentManager.ClientModels
{
    public class PaginationModel<T> where T : class
    {
        public T[] Items { get; set; }
        public int TotalCount { get; set; }
    }
}
