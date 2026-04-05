using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.CustomerProductDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerProductController : ApiController
    {
        private readonly ICustomerProductService _customerProductService;
        private readonly ICustomerPackageService _customerPackageService;

        public CustomerProductController(
            ICustomerProductService customerProductService,
            ICustomerPackageService customerPackageService)
        {
            _customerProductService = customerProductService;
            _customerPackageService = customerPackageService;
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.CustomerProductView)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _customerProductService.GetAllAsync();
            return Ok(ApiResponse<object>.Ok(result));
        }
        [HttpGet("customer/{customerId}")]
        [RequirePermission(PermissionCodes.CustomerProductView)]
        public async Task<IActionResult> GetByCustomerId(int customerId)
        {
            if (customerId <= 0)
                return BadRequest(ApiResponse<object>.Fail("ID không hợp lệ."));

            var userId = TryGetCurrentUserId();
            if (!await _customerPackageService.IsUserAllowedForCustomerAsync(userId, customerId))
                return Forbid();

            var result = await _customerProductService.GetProductsByCustomerIdAsync(customerId);
            return Ok(ApiResponse<object>.Ok(result));
        }
        [HttpGet("product/{productId}")]
        [RequirePermission(PermissionCodes.CustomerProductView)]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            var result = await _customerProductService.GetCustomersByProductIdAsync(productId);
            return Ok(ApiResponse<object>.Ok(result));
        }
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.CustomerProductView)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _customerProductService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy dữ liệu."));
            }
            return Ok(ApiResponse<object>.Ok(result));
        }
        [HttpPost]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerProductDto dto)
        {
            if (dto.Customer_id <= 0 || dto.Product_id <= 0)
                return BadRequest(ApiResponse<object>.Fail("ID không hợp lệ."));

            var userId = TryGetCurrentUserId();
            if (!await _customerPackageService.IsUserAllowedForCustomerAsync(userId, dto.Customer_id))
                return Forbid();

            int createdBy = GetCurrentUserId();
            var result = await _customerProductService.AddProductToCustomerAsync(dto, createdBy);
            if (result == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Sản phẩm đã được gán cho khách hàng này hoặc dữ liệu không hợp lệ."));
            }
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<object>.Ok(result));
        }
        [HttpPost("assign")]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> AssignProducts([FromBody] AssignProductsToCustomerDto dto)
        {
            if (dto.Customer_id <= 0 || dto.Product_ids == null || !dto.Product_ids.Any())
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

            var userId = TryGetCurrentUserId();
            if (!await _customerPackageService.IsUserAllowedForCustomerAsync(userId, dto.Customer_id))
                return Forbid();

            int createdBy = GetCurrentUserId();
            var result = await _customerProductService.AssignProductsToCustomerAsync(dto, createdBy);
            if (!result)
            {
                return BadRequest(ApiResponse<object>.Fail("Không thể gán sản phẩm cho khách hàng. Có thể sản phẩm đã được gán trước đó hoặc dữ liệu gửi lên không hợp lệ."));
            }
            return Ok(ApiResponse<object>.Ok(null, "Gán sản phẩm thành công."));
        }
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerProductDto dto)
        {
            if (id <= 0 || id != dto.Id)
                return BadRequest(ApiResponse<object>.Fail("ID không khớp."));

            var existing = await _customerProductService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy bản ghi."));

            var userId = TryGetCurrentUserId();
            if (!await _customerPackageService.IsUserAllowedForCustomerAsync(userId, existing.Customer_id))
                return Forbid();

            int updatedBy = GetCurrentUserId();
            var result = await _customerProductService.UpdateCustomerProductAsync(dto, updatedBy);
            if (!result)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy bản ghi gán sản phẩm của khách hàng để cập nhật."));
            return Ok(ApiResponse<object>.Ok(null));
        }
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Fail("ID không hợp lệ."));

            var existing = await _customerProductService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy bản ghi."));

            var userId = TryGetCurrentUserId();
            if (!await _customerPackageService.IsUserAllowedForCustomerAsync(userId, existing.Customer_id))
                return Forbid();

            int deletedBy = GetCurrentUserId();
            var result = await _customerProductService.RemoveProductFromCustomerAsync(id, deletedBy);
            if (!result)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy bản ghi gán sản phẩm của khách hàng để xóa."));
            return Ok(ApiResponse<object>.Ok(null));
        }
        [HttpGet("check/{customerId}/{productId}")]
        [RequirePermission(PermissionCodes.CustomerProductView)]
        public async Task<IActionResult> CheckPermission(int customerId, int productId)
        {
            var canOrder = await _customerProductService.CanCustomerOrderProductAsync(customerId, productId);
            return Ok(ApiResponse<object>.Ok(new { canOrder }));
        }

        [HttpPost("exclude/{customerId}/{productId}")]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> ExcludeProduct(int customerId, int productId)
        {
            if (customerId <= 0 || productId <= 0)
                return BadRequest(ApiResponse<object>.Fail("ID không hợp lệ."));

            var userId = TryGetCurrentUserId();
            if (!await _customerPackageService.IsUserAllowedForCustomerAsync(userId, customerId))
                return Forbid();

            int updatedBy = GetCurrentUserId();
            var result = await _customerProductService.ExcludeProductForCustomerAsync(customerId, productId, updatedBy);
            if (!result)
                return BadRequest(ApiResponse<object>.Fail("Không thể loại bỏ sản phẩm."));
            return Ok(ApiResponse<object>.Ok(null, "Đã loại bỏ sản phẩm khỏi gói của khách hàng."));
        }

        [HttpPost("unexclude/{customerId}/{productId}")]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> UnexcludeProduct(int customerId, int productId)
        {
            if (customerId <= 0 || productId <= 0)
                return BadRequest(ApiResponse<object>.Fail("ID không hợp lệ."));

            var userId = TryGetCurrentUserId();
            if (!await _customerPackageService.IsUserAllowedForCustomerAsync(userId, customerId))
                return Forbid();

            int updatedBy = GetCurrentUserId();
            var result = await _customerProductService.UnexcludeProductForCustomerAsync(customerId, productId, updatedBy);
            if (!result)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy bản ghi."));
            return Ok(ApiResponse<object>.Ok(null, "Đã khôi phục sản phẩm trong gói của khách hàng."));
        }

        [HttpGet("excluded/{customerId}")]
        [RequirePermission(PermissionCodes.CustomerProductView)]
        public async Task<IActionResult> GetExcludedProducts(int customerId)
        {
            if (customerId <= 0)
                return BadRequest(ApiResponse<object>.Fail("ID không hợp lệ."));

            var userId = TryGetCurrentUserId();
            if (!await _customerPackageService.IsUserAllowedForCustomerAsync(userId, customerId))
                return Forbid();

            var result = await _customerProductService.GetExcludedProductIdsByCustomerAsync(customerId);
            return Ok(ApiResponse<object>.Ok(result));
        }
    }
}
