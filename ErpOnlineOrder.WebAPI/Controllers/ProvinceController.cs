using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.ProvinceDTOs;
using ErpOnlineOrder.WebAPI.Attributes;

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
            return Ok(provinces);
        }
        [HttpGet("region/{regionId}")]
        [RequirePermission(PermissionCodes.ProvinceView)]
        public async Task<IActionResult> GetProvincesByRegion(int regionId)
        {
            var provinces = await _provinceService.GetByRegionIdAsync(regionId);
            return Ok(provinces);
        }
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.ProvinceView)]
        public async Task<IActionResult> GetProvince(int id)
        {
            var province = await _provinceService.GetByIdAsync(id);
            if (province == null) return NotFound(new { message = "T?nh th?nh kh?ng t?n t?i" });
            return Ok(province);
        }
        [HttpPost]
        [RequirePermission(PermissionCodes.ProvinceCreate)]
        public async Task<IActionResult> CreateProvince([FromBody] CreateProvinceDto dto)
        {
            try
            {
                var created = await _provinceService.CreateProvinceAsync(dto, GetCurrentUserId());
                return CreatedAtAction(nameof(GetProvince), new { id = created?.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.ProvinceUpdate)]
        public async Task<IActionResult> UpdateProvince(int id, [FromBody] UpdateProvinceDto dto)
        {
            if (id != dto.Id) return BadRequest(new { message = "ID kh?ng kh?p" });

            try
            {
                var result = await _provinceService.UpdateProvinceAsync(dto, GetCurrentUserId());
                if (!result) return NotFound(new { message = "T?nh th?nh kh?ng t?n t?i" });
                return Ok(new { success = true, message = "C?p nh?t th?nh c?ng" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.ProvinceDelete)]
        public async Task<IActionResult> DeleteProvince(int id)
        {
            try
            {
                var result = await _provinceService.DeleteProvinceAsync(id);
                if (!result) return NotFound(new { message = "T?nh th?nh kh?ng t?n t?i" });
                return Ok(new { success = true, message = "X?a th?nh c?ng" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}