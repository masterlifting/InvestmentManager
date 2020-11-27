using static InvestmentManager.Client.Configurations.EnumConfig;

namespace InvestmentManager.Client.Configurations
{
    public class ColumnConfig
    {
        public string Caption { get; set; }
        public string DataField { get; set; }
        public HtmlDataType DataType { get; set; }
        public AlignType AlignType { get; set; }
        public string Format { get; set; }
    }
}
