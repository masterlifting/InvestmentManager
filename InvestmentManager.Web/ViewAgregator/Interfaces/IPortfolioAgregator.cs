using InvestmentManager.Web.Models.PortfolioModels;
using System.Collections.Generic;

namespace InvestmentManager.Web.ViewAgregator.Interfaces
{
    public interface IPortfolioAgregator
    {
        StockTransactionErrorForm LoadStockTransactionErrorForm();
        DividendErrorForm LoadDividendErrorForm();
        ResultBrokerReportViewModel GetResultReportView(IEnumerable<PortfolioReportModel> models);
    }
}
