using ExcelDataReader;
using InvestmentManager.Services.Interfaces;
using System.Data;
using System.IO;
using System.Text;

namespace InvestmentManager.Services.Implimentations
{
    public class IOService : IIOService
    {
        public DataSet GetDataSet(Stream stream)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            return excelReader.AsDataSet();
        }
    }
}
