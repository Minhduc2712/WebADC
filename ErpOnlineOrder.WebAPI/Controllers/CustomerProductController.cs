using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.CustomerProductDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerProductController : ApiController
    {
        private readonly ICustomerProductService _customerProductService;

        public CustomerProductController(ICustomerProductService customerProductService)
        {
            _customerProductService = customerProductService;
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.CustomerProductView)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _customerProductService.GetAllAsync();
            return Ok(result);
        }
        [HttpGet("customer/{customerId}")]
        [RequirePermission(PermissionCodes.CustomerProductView)]
        public async Task<IActionResult> GetByCustomerId(int customerId)
        {
            var result = await _customerProductService.GetProductsByCustomerIdAsync(customerId);
            return Ok(result);
        }
        [HttpGet("product/{productId}")]
        [RequirePermission(PermissionCodes.CustomerProductView)]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            var result = await _customerProductService.GetCustomersByProductIdAsync(productId);
            return Ok(result);
        }
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.CustomerProductView)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _customerProductService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        [HttpPost]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerProductDto dto)
        {
            int createdBy = GetCurrentUserId();
            
            var result = await _customerProductService.AddProductToCustomerAsync(dto, createdBy);
            if (result == null)
            {
                return BadRequest("S?n ph?m ?ã ???c gán cho khách hàng này ho?c d? li?u không h?p l?.");
            }
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        [HttpPost("assign")]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> AssignProducts([FromBody] AssignProductsToCustomerDto dto)
        {
            int createdBy = GetCurrentUserId();
            
            var result = await _customerProductService.AssignProductsToCustomerAsync(dto, createdBy);
            if (!result)
            {
                return BadRequest("Không th? gán s?n ph?m cho khách hàng.");
            }
            return Ok(new { message = "Gán s?n ph?m thành công." });
        }
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerProductDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID không kh?p.");
            }

            int updatedBy = GetCurrentUserId();
            
            var result = await _customerProductService.UpdateCustomerProductAsync(dto, updatedBy);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> Delete(int id)
        {
            int deletedBy = GetCurrentUserId();
            
            var result = await _customerProductService.RemoveProductFromCustomerAsync(id, deletedBy);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        [HttpGet("check/{customerId}/{productId}")]
        [RequirePermission(PermissionCodes.CustomerProductView)]
        public async Task<IActionResult> CheckPermission(int customerId, int productId)
        {
            var canOrder = await _customerProductService.CanCustomerOrderProductAsync(customerId, productId);
            return Ok(new { canOrder });
        }
    }
}
