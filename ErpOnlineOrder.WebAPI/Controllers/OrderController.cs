using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using ErpOnlineOrder.Domain.Models;

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
            var orders = await _orderService.GetAllAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto model)
        {
            model.Created_by = GetCurrentUserId();
            var result = await _orderService.CreateOrderAsync(model);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("admin")]
        public async Task<IActionResult> CreateOrderAdmin([FromBody] CreateOrderDto model)
        {
            model.Created_by = GetCurrentUserId();
            var result = await _orderService.CreateOrderWithoutValidationAsync(model);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOrder([FromBody] UpdateOrderDto model)
        {
            try
            {
                model.Updated_by = GetCurrentUserId();
                var result = await _orderService.UpdateOrderAsync(model);
                if (!result) return NotFound();
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var result = await _orderService.DeleteOrderAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/copy")]
        public async Task<IActionResult> CopyOrder(int id)
        {
            try
            {
                var dto = new CopyOrderDto { SourceOrderId = id, Created_by = GetCurrentUserId() };
                var copy = await _orderService.CopyOrderAsync(dto);
                return Ok(copy);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetOrdersByStaff(int staffId)
        {
            var orders = await _orderService.GetOrdersByStaffAsync(staffId);
            return Ok(orders);
        }
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetOrdersByCustomer(int customerId)
        {
            var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
            return Ok(orders);
        }
        [HttpGet("customer/{customerId}/allowed-products")]
        public async Task<IActionResult> GetAllowedProducts(int customerId)
        {
            var products = await _orderService.GetAllowedProductsForCustomerAsync(customerId);
            return Ok(products);
        }
        [HttpGet("customer/{customerId}/can-order/{productId}")]
        public async Task<IActionResult> CanCustomerOrderProduct(int customerId, int productId)
        {
            var canOrder = await _orderService.CanCustomerOrderProductAsync(customerId, productId);
            return Ok(new { can_order = canOrder });
        }
        [HttpGet("customer/{customerId}/product-price/{productId}")]
        public async Task<IActionResult> GetProductPrice(int customerId, int productId)
        {
            var price = await _orderService.GetProductPriceForCustomerAsync(customerId, productId);
            return Ok(new { price });
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmOrder(int id)
        {
            try
            {
                var dto = new ConfirmOrderDto { OrderId = id, Updated_by = GetCurrentUserId() };
                var result = await _orderService.ConfirmOrderAsync(dto);
                if (!result) return NotFound();
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var dto = new CancelOrderDto { OrderId = id, Updated_by = GetCurrentUserId() };
                var result = await _orderService.CancelOrderAsync(dto);
                if (!result) return NotFound();
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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
            var orders = await _orderService.GetOrdersByStatusAsync(status);
            return Ok(orders);
        }

        [HttpDelete("pending/{id}")]
        public async Task<IActionResult> DeletePendingOrder(int id)
        {
            try
            {
                var result = await _orderService.DeletePendingOrderAsync(id);
                if (!result) return NotFound();
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}