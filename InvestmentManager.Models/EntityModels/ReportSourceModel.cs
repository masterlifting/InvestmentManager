using InvestmentManager.Models.Interfaces;

namespace InvestmentManager.Models.EntityModels
{
    public class ReportSourceModel : IEditebleModel
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public bool IsEditeble { get; init; }
    }
}
