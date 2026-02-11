using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.CustomerManagementDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    public class AssignStaffRequest
    {
        public int Staff_id { get; set; }
        public int Customer_id { get; set; }
        public int Province_id { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class CustomerManagementController : ApiController
    {
        private readonly ICustomerManagementService _customerManagementService;

        public CustomerManagementController(ICustomerManagementService customerManagementService)
        {
            _customerManagementService = customerManagementService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerManagements([FromQuery] int? staffId, [FromQuery] int? provinceId)
        {
            IEnumerable<Customer_management> customerManagements;
            if (staffId.HasValue)
            {
                customerManagements = await _customerManagementService.GetByStaffAsync(staffId.Value);
            }
            else if (provinceId.HasValue)
            {
                customerManagements = await _customerManagementService.GetByProvinceAsync(provinceId.Value);
            }
            else
            {
                customerManagements = await _customerManagementService.GetAllAsync();
            }
            return Ok(customerManagements);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var list = await _customerManagementService.GetByCustomerAsync(customerId);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerManagement(int id)
        {
            var customerManagement = await _customerManagementService.GetByIdAsync(id);
            if (customerManagement == null) return NotFound();
            return Ok(customerManagement);
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignStaff([FromBody] AssignStaffRequest request)
        {
            var createdBy = GetCurrentUserId();
            try
            {
                var created = await _customerManagementService.AssignStaffToCustomerAsync(
                    request.Staff_id, request.Customer_id, request.Province_id, createdBy);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("check")]
        public async Task<IActionResult> IsAlreadyAssigned([FromQuery] int staffId, [FromQuery] int customerId)
        {
            var result = await _customerManagementService.IsAlreadyAssignedAsync(staffId, customerId);
            return Ok(new { already_assigned = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomerManagement([FromBody] CreateCustomerManagementDto dto)
        {
            try
            {
                var created = await _customerManagementService.CreateCustomerManagementAsync(dto, GetCurrentUserId());
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomerManagement(int id, [FromBody] UpdateCustomerManagementDto dto)
        {
            if (id != dto.Id) return BadRequest();

            try
            {
                var result = await _customerManagementService.UpdateCustomerManagementAsync(id, dto, GetCurrentUserId());
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomerManagement(int id)
        {
            // Require permission: MANAGE_CUSTOMER_MANAGEMENT
            try
            {
                var result = await _customerManagementService.DeleteCustomerManagementAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}