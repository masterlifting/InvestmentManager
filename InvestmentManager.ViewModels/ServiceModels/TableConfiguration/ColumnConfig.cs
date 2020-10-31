namespace InvestmentManager.ViewModels.ServiceModels.TableConfiguration
{
    public class ColumnConfig
    {
        public ColumnConfig() => DataType = Enums.DataType.NotSet;

        public string Caption { get; set; }
        public string DataField { get; set; }
        public Enums.DataType DataType { get; set; }
        public Enums.Alignment Align { get; set; }
        public string Format { get; set; }
    }
}
