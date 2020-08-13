using System.Data;

namespace InvestmentManager.Service.Interfaces
{
    public interface IIOService
    {
        DataSet LoadDataSetFromExcel(string path);
    }
}
