namespace InvestmentManager.ClientModels
{
    public class ClientBaseResponse<T> where T : class
    {
        public T Data { get; set; }
        public bool IsSuccess { get; set; }
        public string[] Errors { get; set; }
    }
}
