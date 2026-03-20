using ErpOnlineOrder.Application.DTOs.RegionDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegionController : ApiController
    {
        private readonly IRegionService _regionService;

        public RegionController(IRegionService regionService)
        {
            _regionService = regionService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var regions = await _regionService.GetAllAsync();
            return Ok(ApiResponse<object>.Ok(regions));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var region = await _regionService.GetByIdAsync(id);
            if (region == null)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy khu vực."));
            }
            return Ok(ApiResponse<object>.Ok(region));
        }
        [HttpPost]
        public async Task<IActionResult> CreateRegion([FromBody] CreateRegionDto dto)
        {
            try
            {
                var created = await _regionService.CreateRegionAsync(dto, GetCurrentUserId());
                if (created == null)
                {
                    return BadRequest(ApiResponse<object>.Fail("Không thể tạo khu vực. Mã khu vực có thể đã tồn tại hoặc dữ liệu chưa hợp lệ."));
                }
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<object>.Ok(created));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRegion(int id, [FromBody] UpdateRegionDto dto)
        {
            try
            {
                dto.Id = id;
                var result = await _regionService.UpdateRegionAsync(dto, GetCurrentUserId());
                if (!result)
                {
                    return NotFound(ApiResponse<object>.Fail("Không tìm thấy khu vực cần cập nhật."));
                }
                return Ok(ApiResponse<object>.Ok(null, "Cập nhật khu vực thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegion(int id)
        {
            var result = await _regionService.DeleteRegionAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy khu vực cần xóa."));
            }
            return Ok(ApiResponse<object>.Ok(null, "Xóa khu vực thành công"));
        }
    }
}