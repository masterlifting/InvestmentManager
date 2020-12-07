namespace InvestmentManager.Models
{
    public class BaseActionResult
    {
        public bool IsSuccess { get; set; } = false;
        public string Info { get; set; }
        public long ResultId { get; init; }
    }
}
