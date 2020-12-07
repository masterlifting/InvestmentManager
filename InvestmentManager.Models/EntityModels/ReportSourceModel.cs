namespace InvestmentManager.Models.EntityModels
{
    public class ReportSourceModel
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
