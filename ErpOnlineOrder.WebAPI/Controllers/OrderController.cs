using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Application.DTOs;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ApiController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _orderService.GetAllAsync(TryGetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(orders));
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetOrdersPaged([FromQuery] OrderFilterRequest request)
        {
            var result = await _orderService.GetAllPagedAsync(request, TryGetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderService.GetByIdAsync(id, TryGetCurrentUserId());
            if (order == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy đơn hàng."));
            return Ok(ApiResponse<object>.Ok(order));
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto model)
        {
            model.Created_by = GetCurrentUserId();
            var result = await _orderService.CreateOrderAsync(model);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Lỗi khi tạo đơn hàng."));
            }
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpPost("admin")]
        public IActionResult CreateOrderAdmin() => StatusCode(410, ApiResponse<object>.Fail("Chức năng tạo đơn hàng Admin đã tạm đóng."));

        [HttpPut]
        public async Task<IActionResult> UpdateOrder([FromBody] UpdateOrderDto model)
        {
            try
            {
                model.Updated_by = GetCurrentUserId();
                var result = await _orderService.UpdateOrderAsync(model);
                if (!result.Success)
                {
                    return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Lỗi khi cập nhật đơn hàng."));
                }
                return Ok(ApiResponse<object>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var result = await _orderService.DeleteOrderAsync(id);
                return Ok(ApiResponse<object>.Ok(new { success = result }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("{id}/copy")]
        public async Task<IActionResult> CopyOrder(int id)
        {
            try
            {
                var dto = new CopyOrderDto { SourceOrderId = id, Created_by = GetCurrentUserId() };
                var copy = await _orderService.CopyOrderAsync(dto);
                return Ok(ApiResponse<object>.Ok(copy));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetOrdersByStaff(int staffId)
        {
            var orders = await _orderService.GetOrdersByStaffAsync(staffId);
            return Ok(ApiResponse<object>.Ok(orders));
        }
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetOrdersByCustomer(int customerId)
        {
            var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
            return Ok(ApiResponse<IEnumerable<OrderDTO>>.Ok(orders));
        }

        [HttpGet("customer/{customerId}/paged")]
        public async Task<IActionResult> GetOrdersByCustomerPaged(int customerId, [FromQuery] OrderFilterRequest request)
        {
            var paged = await _orderService.GetOrdersByCustomerPagedAsync(customerId, request);
            return Ok(ApiResponse<PagedResult<OrderDTO>>.Ok(paged));
        }

        [HttpPost("customer")]
        public async Task<IActionResult> CreateOrderCustomer([FromBody] CreateOrderDto model)
        {
            var result = await _orderService.CreateOrderWithoutValidationAsync(model);
            if (result.Success)
                return Ok(ApiResponse<CreateOrderResultDto>.Ok(result));
            return BadRequest(ApiResponse<CreateOrderResultDto>.Fail(result.Message));
        }

        [HttpGet("customer/{customerId}/allowed-products")]
        public async Task<IActionResult> GetAllowedProducts(int customerId)
        {
            var products = await _orderService.GetAllowedProductsForCustomerAsync(customerId);
            return Ok(ApiResponse<object>.Ok(products));
        }
        [HttpGet("customer/{customerId}/can-order/{productId}")]
        public async Task<IActionResult> CanCustomerOrderProduct(int customerId, int productId)
        {
            var canOrder = await _orderService.CanCustomerOrderProductAsync(customerId, productId);
            return Ok(ApiResponse<object>.Ok(new { can_order = canOrder }));
        }
        [HttpGet("customer/{customerId}/product-price/{productId}")]
        public async Task<IActionResult> GetProductPrice(int customerId, int productId)
        {
            var price = await _orderService.GetProductPriceForCustomerAsync(customerId, productId);
            return Ok(ApiResponse<object>.Ok(new { price }));
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmOrder(int id, [FromBody] ConfirmOrderDto? request)
        {
            try
            {
                var dto = request ?? new ConfirmOrderDto();
                dto.OrderId = id;
                dto.Updated_by = GetCurrentUserId();
                var result = await _orderService.ConfirmOrderAsync(dto);
                if (!result.Success)
                {
                    return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Lỗi xác nhận đơn."));
                }

                return Ok(ApiResponse<object>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var dto = new CancelOrderDto { OrderId = id, Updated_by = GetCurrentUserId() };
                var result = await _orderService.CancelOrderAsync(dto);
                if (!result) return NotFound(ApiResponse<object>.Fail("Không tìm thấy đơn hàng."));
                return Ok(ApiResponse<object>.Ok(new { success = result }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportOrders()
        {
            var excelData = await _orderService.ExportOrdersToExcelAsync();
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "orders.xlsx");
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetOrdersByStatus(string status)
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status, TryGetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(orders));
        }

        [HttpDelete("pending/{id}")]
        public async Task<IActionResult> DeletePendingOrder(int id)
        {
            try
            {
                var result = await _orderService.DeletePendingOrderAsync(id);
                if (!result) return NotFound(ApiResponse<object>.Fail("Không tìm thấy đơn hàng."));
                return Ok(ApiResponse<object>.Ok(new { success = result }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
