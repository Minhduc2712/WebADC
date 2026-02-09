using ErpOnlineOrder.Application.DTOs.RegionDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegionController : ControllerBase
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
            return Ok(regions);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var region = await _regionService.GetByIdAsync(id);
            if (region == null)
            {
                return NotFound(new { message = "Khu v?c không t?n t?i" });
            }
            return Ok(region);
        }
        [HttpPost]
        public async Task<IActionResult> CreateRegion([FromBody] CreateRegionDto dto)
        {
            try
            {
                // TODO: L?y userId t? token
                int createdBy = 1;

                var created = await _regionService.CreateRegionAsync(dto, createdBy);
                if (created == null)
                {
                    return BadRequest(new { message = "Không th? t?o khu v?c" });
                }
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRegion(int id, [FromBody] UpdateRegionDto dto)
        {
            try
            {
                dto.Id = id;
                // TODO: L?y userId t? token
                int updatedBy = 1;

                var result = await _regionService.UpdateRegionAsync(dto, updatedBy);
                if (!result)
                {
                    return NotFound(new { message = "Khu v?c không t?n t?i" });
                }
                return Ok(new { success = true, message = "C?p nh?t khu v?c thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegion(int id)
        {
            var result = await _regionService.DeleteRegionAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Khu v?c không t?n t?i" });
            }
            return Ok(new { success = true, message = "Xóa khu v?c thành công" });
        }
    }
}