using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ApiController
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(customers.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();
            return Ok(MapToDto(customer));
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
        {
            try
            {
                var created = await _customerService.CreateCustomerAsync(dto, GetCurrentUserId());
                return Ok(MapToDto(created));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerByAdminDto dto)
        {
            if (id != dto.Id) return BadRequest();
            try
            {
                var result = await _customerService.UpdateCustomerByAdminAsync(id, dto, GetCurrentUserId());
                if (!result) return NotFound();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static CustomerDTO MapToDto(Customer c)
        {
            return new CustomerDTO
            {
                Id = c.Id,
                Customer_code = c.Customer_code ?? "",
                Full_name = c.Full_name ?? "",
                Phone_number = c.Phone_number ?? "",
                Address = c.Address ?? "",
                Created_at = c.Created_at,
                Updated_at = c.Updated_at,
                Is_deleted = c.Is_deleted,
                Username = c.User?.Username,
                Email = c.User?.Email
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                await _customerService.DeleteAsync(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update-customer")]
        public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerDto model)
        {
            try
            {
                var result = await _customerService.UpdateCustomerAsync(model);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update-organization")]
        public async Task<IActionResult> UpdateOrganization([FromBody] UpdateOrganizationByCustomerDto model)
        {
            try
            {
                var result = await _customerService.UpdateOrganizationAsync(model);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}