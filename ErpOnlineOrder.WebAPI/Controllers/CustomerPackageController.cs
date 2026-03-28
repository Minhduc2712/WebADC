using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.CustomerPackageDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerPackageController : ApiController
    {
        private readonly ICustomerPackageService _service;

        public CustomerPackageController(ICustomerPackageService service)
        {
            _service = service;
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.CustomerPackageView)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("paged")]
        [RequirePermission(PermissionCodes.CustomerPackageView)]
        public async Task<IActionResult> GetPaged(int page = 1, int pageSize = 20, string? search = null)
        {
            // Truyền userId xuống service, service tự xác định admin hay nhân viên
            var userId = TryGetCurrentUserId();
            var result = await _service.GetPagedAsync(page, pageSize, search, userId);
            return Ok(ApiResponse<PagedResult<CustomerPackageDto>>.Ok(result));
        }

        [HttpGet("customer/{customerId}")]
        [RequirePermission(PermissionCodes.CustomerPackageView)]
        public async Task<IActionResult> GetByCustomerId(int customerId)
        {
            var userId = TryGetCurrentUserId();
            if (!await _service.IsUserAllowedForCustomerAsync(userId, customerId))
                return Forbid();
            var result = await _service.GetByCustomerIdAsync(customerId);
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("package/{packageId}")]
        [RequirePermission(PermissionCodes.CustomerPackageView)]
        public async Task<IActionResult> GetByPackageId(int packageId)
        {
            var result = await _service.GetByPackageIdAsync(packageId);
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.CustomerPackageView)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy dữ liệu."));
            var userId = TryGetCurrentUserId();
            if (!await _service.IsUserAllowedForCustomerAsync(userId, result.Customer_id))
                return Forbid();
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpPost]
        [RequirePermission(PermissionCodes.CustomerPackageAssign)]
        public async Task<IActionResult> Assign([FromBody] CreateCustomerPackageDto dto)
        {
            var userId = TryGetCurrentUserId();
            if (!await _service.IsUserAllowedForCustomerAsync(userId, dto.Customer_id))
                return Forbid();
            var result = await _service.AssignPackageToCustomerAsync(dto, GetCurrentUserId());
            if (result == null)
                return BadRequest(ApiResponse<object>.Fail("Gói đã được gán cho khách hàng này."));
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<object>.Ok(result));
        }

        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.CustomerPackageAssign)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerPackageDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<object>.Fail("ID không khớp."));
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy bản ghi."));
            var userId = TryGetCurrentUserId();
            if (!await _service.IsUserAllowedForCustomerAsync(userId, existing.Customer_id))
                return Forbid();
            var ok = await _service.UpdateAsync(id, dto, GetCurrentUserId());
            if (!ok)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy bản ghi."));
            return Ok(ApiResponse<object>.Ok(string.Empty));
        }

        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.CustomerPackageAssign)]
        public async Task<IActionResult> Unassign(int id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy bản ghi."));
            var userId = TryGetCurrentUserId();
            if (!await _service.IsUserAllowedForCustomerAsync(userId, existing.Customer_id))
                return Forbid();
            var ok = await _service.UnassignPackageFromCustomerAsync(id, GetCurrentUserId());
            if (!ok)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy bản ghi."));
            return Ok(ApiResponse<object>.Ok(string.Empty, "Đã hủy gán gói thành công."));
        }
    }
}
