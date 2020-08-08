using System.Data;

namespace InvestmentManager.Service.Interfaces
{
    public interface ILoaderService
    {
        DataSet LoadDataSetFromExcel(string path);
    }
}
