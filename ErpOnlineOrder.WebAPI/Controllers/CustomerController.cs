using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Services;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ApiController
    {
        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;

        public CustomerController(ICustomerService customerService, IPermissionService permissionService)
        {
            _customerService = customerService;
            _permissionService = permissionService;
        }

        [HttpGet("for-select")]
        public async Task<IActionResult> GetForSelect()
        {
            var list = await _customerService.GetForSelectAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _customerService.GetAllAsync();
            var list = customers.Select(EntityMappers.ToCustomerDto).ToList();
            var userId = TryGetCurrentUserId();
            if (userId.HasValue && userId.Value > 0)
            {
                var permissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in list)
                    RecordPermissionEnricher.Enrich(dto, permissions, PermissionCodes.CustomerUpdate, PermissionCodes.CustomerDelete);
            }
            return Ok(list);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetCustomersPaged([FromQuery] CustomerFilterRequest request)
        {
            var result = await _customerService.GetAllPagedAsync(request, TryGetCurrentUserId());
            var userId = TryGetCurrentUserId();
            if (userId.HasValue && userId.Value > 0)
            {
                var permissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in result.Items)
                    RecordPermissionEnricher.Enrich(dto, permissions, PermissionCodes.CustomerUpdate, PermissionCodes.CustomerDelete);
            }
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();
            var dto = EntityMappers.ToCustomerDto(customer);
            var userId = TryGetCurrentUserId();
            if (userId.HasValue && userId.Value > 0)
                await RecordPermissionEnricher.EnrichAsync(dto, userId.Value, _permissionService, PermissionCodes.CustomerUpdate, PermissionCodes.CustomerDelete);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
        {
            try
            {
                var created = await _customerService.CreateCustomerAsync(dto, GetCurrentUserId());
                return Ok(EntityMappers.ToCustomerDto(created));
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
                var userId = TryGetCurrentUserId();
                if (!userId.HasValue || userId.Value <= 0)
                    return Unauthorized(new { message = "Vui lòng đăng nhập." });

                var customer = await _customerService.GetByIdAsync(model.Customer_id);
                if (customer == null)
                    return NotFound(new { message = "Không tìm thấy khách hàng." });
                if (customer.User_id != userId.Value)
                    return Forbid();

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