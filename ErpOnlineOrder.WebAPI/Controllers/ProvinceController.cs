using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.ProvinceDTOs;
using ErpOnlineOrder.WebAPI.Attributes;
using ErpOnlineOrder.Application.DTOs;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProvinceController : ApiController
    {
        private readonly IProvinceService _provinceService;

        public ProvinceController(IProvinceService provinceService)
        {
            _provinceService = provinceService;
        }
        [HttpGet]
        [RequirePermission(PermissionCodes.ProvinceView)]
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await _provinceService.GetAllAsync();
            return Ok(ApiResponse<object>.Ok(provinces));
        }
        [HttpGet("region/{regionId}")]
        [RequirePermission(PermissionCodes.ProvinceView)]
        public async Task<IActionResult> GetProvincesByRegion(int regionId)
        {
            var provinces = await _provinceService.GetByRegionIdAsync(regionId);
            return Ok(ApiResponse<object>.Ok(provinces));
        }
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.ProvinceView)]
        public async Task<IActionResult> GetProvince(int id)
        {
            var province = await _provinceService.GetByIdAsync(id);
            if (province == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy tỉnh/thành phố."));
            return Ok(ApiResponse<object>.Ok(province));
        }
        [HttpPost]
        [RequirePermission(PermissionCodes.ProvinceCreate)]
        public async Task<IActionResult> CreateProvince([FromBody] CreateProvinceDto dto)
        {
            try
            {
                var created = await _provinceService.CreateProvinceAsync(dto, GetCurrentUserId());
                return CreatedAtAction(nameof(GetProvince), new { id = created?.Id }, ApiResponse<object>.Ok(created));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.ProvinceUpdate)]
        public async Task<IActionResult> UpdateProvince(int id, [FromBody] UpdateProvinceDto dto)
        {
            if (id != dto.Id) return BadRequest(ApiResponse<object>.Fail("ID trên URL không khớp với dữ liệu cập nhật tỉnh/thành phố."));

            try
            {
                var result = await _provinceService.UpdateProvinceAsync(dto, GetCurrentUserId());
                if (!result) return NotFound(ApiResponse<object>.Fail("Không tìm thấy tỉnh/thành phố cần cập nhật."));
                return Ok(ApiResponse<object>.Ok(null, "Cập nhật tỉnh/thành phố thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.ProvinceDelete)]
        public async Task<IActionResult> DeleteProvince(int id)
        {
            try
            {
                var result = await _provinceService.DeleteProvinceAsync(id);
                if (!result) return NotFound(ApiResponse<object>.Fail("Không tìm thấy tỉnh/thành phố cần xóa."));
                return Ok(ApiResponse<object>.Ok(null, "Xóa tỉnh/thành phố thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}