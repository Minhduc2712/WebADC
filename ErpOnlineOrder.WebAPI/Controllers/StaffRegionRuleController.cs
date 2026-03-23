using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.StaffRegionRuleDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffRegionRuleController : ApiController
    {
        private readonly IStaffRegionRuleService _staffRegionRuleService;

        public StaffRegionRuleController(IStaffRegionRuleService staffRegionRuleService)
        {
            _staffRegionRuleService = staffRegionRuleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? staffId, [FromQuery] int? provinceId)
        {
            var rules = await _staffRegionRuleService.GetAllAsync(staffId, provinceId);
            return Ok(ApiResponse<object>.Ok(rules));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rule = await _staffRegionRuleService.GetByIdAsync(id);
            if (rule == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy quy tắc phân công"));
            return Ok(ApiResponse<object>.Ok(rule));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStaffRegionRuleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));

            try
            {
                var created = await _staffRegionRuleService.CreateAsync(dto, GetCurrentUserId());
                return Ok(ApiResponse<object>.Ok(created));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStaffRegionRuleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));

            try
            {
                var updated = await _staffRegionRuleService.UpdateAsync(id, dto, GetCurrentUserId());
                return Ok(ApiResponse<object>.Ok(updated));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _staffRegionRuleService.DeleteAsync(id);
                return Ok(ApiResponse<object>.Ok("Đã xóa quy tắc phân công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
