namespace InvestmentManager.ViewModels.ErrorModels
{
    public class ErrorBaseModel
    {
        public bool IsSuccess { get; set; } = false;
        public string[] Errors { get; set; }
    }
}
