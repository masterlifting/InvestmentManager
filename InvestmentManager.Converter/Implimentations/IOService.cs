using ExcelDataReader;
using InvestmentManager.Service.Interfaces;
using System.Data;
using System.IO;
using System.Text;

namespace InvestmentManager.Service.Implimentations
{
    public class IOService : IIOService
    {
        public DataSet LoadDataSetFromExcel(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using Stream reader = File.Open(path, FileMode.Open, FileAccess.Read);
            using IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(reader);
            using DataSet report = excelReader.AsDataSet();

            reader.Close();
            excelReader.Close();

            return report;
        }
    }
}
