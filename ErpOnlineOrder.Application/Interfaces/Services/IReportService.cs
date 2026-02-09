using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IReportService
    {
        Task<Dictionary<string, int>> GetOrderStatusSummaryAsync();
        Task<Dictionary<string, decimal>> GetOrderRevenueByMonthAsync(int year);
        Task<IEnumerable<dynamic>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}