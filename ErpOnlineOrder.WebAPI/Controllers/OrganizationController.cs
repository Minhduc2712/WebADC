using ErpOnlineOrder.Application.DTOs.OrganizationDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationController : ApiController
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var organizations = await _organizationService.GetAllAsync();
            return Ok(organizations);
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? keyword)
        {
            var organizations = await _organizationService.SearchAsync(keyword);
            return Ok(organizations);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var organization = await _organizationService.GetByIdAsync(id);
            if (organization == null)
            {
                return NotFound(new { message = "Không tìm thấy tổ chức." });
            }
            return Ok(organization);
        }
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var organization = await _organizationService.GetByCustomerIdAsync(customerId);
            if (organization == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin tổ chức của khách hàng này." });
            }
            return Ok(organization);
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationDto dto)
        {
            try
            {
                var created = await _organizationService.CreateOrganizationAsync(dto, GetCurrentUserId());
                if (created == null)
                {
                    return BadRequest(new { message = "Không thể tạo tổ chức. Mã số thuế hoặc thông tin tổ chức có thể không hợp lệ/trùng lặp." });
                }
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrganization(int id, [FromBody] UpdateOrganizationDto dto)
        {
            try
            {
                dto.Id = id;
                var result = await _organizationService.UpdateOrganizationAsync(dto, GetCurrentUserId());
                if (!result)
                {
                    return NotFound(new { message = "Không tìm thấy tổ chức cần cập nhật." });
                }
                return Ok(new { success = true, message = "Cập nhật tổ chức thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            var result = await _organizationService.DeleteOrganizationAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy tổ chức cần xóa." });
            }
            return Ok(new { success = true, message = "Xóa tổ chức thành công" });
        }
    }
}