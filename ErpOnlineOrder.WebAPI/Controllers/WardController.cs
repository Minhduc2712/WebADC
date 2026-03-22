using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.WardDTOs;
using ErpOnlineOrder.WebAPI.Attributes;
using ErpOnlineOrder.Application.DTOs;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WardController : ApiController
    {
        private readonly IWardService _wardService;

        public WardController(IWardService wardService)
        {
            _wardService = wardService;
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.WardView)]
        public async Task<IActionResult> GetWards()
        {
            var wards = await _wardService.GetAllAsync();
            return Ok(ApiResponse<object>.Ok(wards));
        }

        [HttpGet("province/{provinceId}")]
        public async Task<IActionResult> GetWardsByProvince(int provinceId)
        {
            var wards = await _wardService.GetByProvinceIdAsync(provinceId);
            return Ok(ApiResponse<object>.Ok(wards));
        }

        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.WardView)]
        public async Task<IActionResult> GetWard(int id)
        {
            var ward = await _wardService.GetByIdAsync(id);
            if (ward == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy phường/xã."));
            return Ok(ApiResponse<object>.Ok(ward));
        }

        [HttpPost]
        [RequirePermission(PermissionCodes.WardCreate)]
        public async Task<IActionResult> CreateWard([FromBody] CreateWardDto dto)
        {
            try
            {
                var created = await _wardService.CreateWardAsync(dto, GetCurrentUserId());
                return CreatedAtAction(nameof(GetWard), new { id = created?.Id }, ApiResponse<object?>.Ok(created));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.WardUpdate)]
        public async Task<IActionResult> UpdateWard(int id, [FromBody] UpdateWardDto dto)
        {
            if (id != dto.Id) return BadRequest(ApiResponse<object>.Fail("ID trên URL không khớp với dữ liệu cập nhật phường/xã."));

            try
            {
                var result = await _wardService.UpdateWardAsync(id, dto, GetCurrentUserId());
                if (!result) return NotFound(ApiResponse<object>.Fail("Không tìm thấy phường/xã cần cập nhật."));
                return Ok(ApiResponse<object>.Ok(new { }, "Cập nhật phường/xã thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.WardDelete)]
        public async Task<IActionResult> DeleteWard(int id)
        {
            try
            {
                var result = await _wardService.DeleteWardAsync(id);
                if (!result) return NotFound(ApiResponse<object>.Fail("Không tìm thấy phường/xã cần xóa."));
                return Ok(ApiResponse<object>.Ok(new { }, "Xóa phường/xã thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
