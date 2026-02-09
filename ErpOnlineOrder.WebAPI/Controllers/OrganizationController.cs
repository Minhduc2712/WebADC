using ErpOnlineOrder.Application.DTOs.OrganizationDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationController : ControllerBase
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
                return NotFound(new { message = "T? ch?c không t?n t?i" });
            }
            return Ok(organization);
        }
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var organization = await _organizationService.GetByCustomerIdAsync(customerId);
            if (organization == null)
            {
                return NotFound(new { message = "Không tìm th?y t? ch?c cho khách hàng này" });
            }
            return Ok(organization);
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationDto dto)
        {
            try
            {
                // TODO: L?y userId t? token
                int createdBy = 1;

                var created = await _organizationService.CreateOrganizationAsync(dto, createdBy);
                if (created == null)
                {
                    return BadRequest(new { message = "Không th? t?o t? ch?c" });
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
                // TODO: L?y userId t? token
                int updatedBy = 1;

                var result = await _organizationService.UpdateOrganizationAsync(dto, updatedBy);
                if (!result)
                {
                    return NotFound(new { message = "T? ch?c không t?n t?i" });
                }
                return Ok(new { success = true, message = "C?p nh?t t? ch?c thành công" });
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
                return NotFound(new { message = "T? ch?c không t?n t?i" });
            }
            return Ok(new { success = true, message = "Xóa t? ch?c thành công" });
        }
    }
}