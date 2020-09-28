using System.Data;
using System.IO;

namespace InvestManager.Services.Interfaces
{
    public interface IIOService
    {
        DataSet GetDataSet(Stream stream);
    }
}
