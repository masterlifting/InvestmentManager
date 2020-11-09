namespace InvestmentManager.ViewModels.ResultModels
{
    public class ResultBaseModel
    {
        public bool IsSuccess { get; set; } = false;
        public string[] Errors { get; set; }
    }
}
