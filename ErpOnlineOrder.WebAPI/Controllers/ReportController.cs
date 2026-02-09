using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("order-status-summary")]
        public async Task<IActionResult> GetOrderStatusSummary()
        {
            // Require permission: VIEW_REPORTS
            var summary = await _reportService.GetOrderStatusSummaryAsync();
            return Ok(summary);
        }

        [HttpGet("revenue-by-month/{year}")]
        public async Task<IActionResult> GetOrderRevenueByMonth(int year)
        {
            // Require permission: VIEW_REPORTS
            var revenue = await _reportService.GetOrderRevenueByMonthAsync(year);
            return Ok(revenue);
        }

        [HttpGet("orders-by-date-range")]
        public async Task<IActionResult> GetOrdersByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            // Require permission: VIEW_REPORTS
            var orders = await _reportService.GetOrdersByDateRangeAsync(startDate, endDate);
            return Ok(orders);
        }
    }
}