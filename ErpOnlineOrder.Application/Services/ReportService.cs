using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IOrderRepository _orderRepository;

        public ReportService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Dictionary<string, int>> GetOrderStatusSummaryAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.GroupBy(o => o.Order_status)
                         .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, decimal>> GetOrderRevenueByMonthAsync(int year)
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Where(o => o.Order_date.Year == year)
                         .GroupBy(o => o.Order_date.Month)
                         .ToDictionary(g => g.Key.ToString(), g => g.Sum(o => o.Total_price));
        }

        public async Task<IEnumerable<dynamic>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Where(o => o.Order_date >= startDate && o.Order_date <= endDate)
                         .Select(o => new
                         {
                             o.Id,
                             o.Order_code,
                             o.Order_date,
                             o.Total_price,
                             o.Order_status,
                             CustomerName = o.Customer?.Full_name
                         });
        }
    }
}