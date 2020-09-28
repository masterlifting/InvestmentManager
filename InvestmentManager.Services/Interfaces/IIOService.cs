using System.Data;
using System.IO;

namespace InvestmentManager.Services.Interfaces
{
    public interface IIOService
    {
        DataSet GetDataSet(Stream stream);
    }
}
