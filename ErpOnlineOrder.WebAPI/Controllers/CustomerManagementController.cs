using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerManagementController : ControllerBase
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerManagement(int id)
        {
            var customerManagement = await _customerManagementService.GetByIdAsync(id);
            if (customerManagement == null) return NotFound();
            return Ok(customerManagement);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomerManagement([FromBody] Customer_management model)
        {
            // Require permission: MANAGE_CUSTOMER_MANAGEMENT
            try
            {
                var created = await _customerManagementService.CreateCustomerManagementAsync(model);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomerManagement(int id, [FromBody] Customer_management model)
        {
            // Require permission: MANAGE_CUSTOMER_MANAGEMENT
            if (id != model.Id) return BadRequest();

            try
            {
                var result = await _customerManagementService.UpdateCustomerManagementAsync(model);
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