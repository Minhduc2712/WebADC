using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Services;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Application.DTOs.OrganizationDTOs;
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
        private readonly IOrganizationService _organizationService;
        private readonly ICustomerRepository _customerRepository;

        public CustomerController(ICustomerService customerService, IPermissionService permissionService, IOrganizationService organizationService, ICustomerRepository customerRepository)
        {
            _customerService = customerService;
            _permissionService = permissionService;
            _organizationService = organizationService;
            _customerRepository = customerRepository;
        }

        [HttpGet("for-select")]
        public async Task<IActionResult> GetForSelect()
        {
            var list = await _customerService.GetForSelectAsync();
            return Ok(ApiResponse<object>.Ok(list));
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
            return Ok(ApiResponse<object>.Ok(list));
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetCustomersPaged([FromQuery] CustomerFilterRequest request)
        {
            var userId = TryGetCurrentUserId();
            var result = await _customerService.GetAllPagedAsync(request, userId);
            if (userId.HasValue && userId.Value > 0)
            {
                var permissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in result.Items)
                    RecordPermissionEnricher.Enrich(dto, permissions, PermissionCodes.CustomerUpdate, PermissionCodes.CustomerDelete);
            }
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy khách hàng."));
            var dto = EntityMappers.ToCustomerDto(customer);
            var userId = TryGetCurrentUserId();
            if (userId.HasValue && userId.Value > 0)
                await RecordPermissionEnricher.EnrichAsync(dto, userId.Value, _permissionService, PermissionCodes.CustomerUpdate, PermissionCodes.CustomerDelete);
            return Ok(ApiResponse<object>.Ok(dto));
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
        {
            try
            {
                var created = await _customerService.CreateCustomerAsync(dto, GetCurrentUserId());
                return Ok(ApiResponse<object>.Ok(EntityMappers.ToCustomerDto(created)));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerByAdminDto dto)
        {
            if (id != dto.Id) return BadRequest(ApiResponse<object>.Fail("ID không khớp."));
            try
            {
                var result = await _customerService.UpdateCustomerByAdminAsync(id, dto, GetCurrentUserId());
                if (!result) return NotFound(ApiResponse<object>.Fail("Không tìm thấy khách hàng cần cập nhật."));
                return Ok(ApiResponse<object>.Ok(new { success = true }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                await _customerService.DeleteAsync(id);
                return Ok(ApiResponse<object>.Ok(new { success = true }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("update-customer")]
        public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerDto model)
        {
            try
            {
                var userId = TryGetCurrentUserId();
                if (!userId.HasValue || userId.Value <= 0)
                    return Unauthorized(ApiResponse<object>.Fail("Vui lòng đăng nhập."));

                var result = await _customerService.UpdateCustomerAsync(model);
                return Ok(ApiResponse<object>.Ok(new { success = result }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return NotFound(ApiResponse<CustomerDTO>.Fail("Không tìm thấy khách hàng."));
            return Ok(ApiResponse<CustomerDTO>.Ok(EntityMappers.ToCustomerDto(customer)));
        }

        [HttpGet("{customerId}/organization")]
        public async Task<IActionResult> GetOrganizationByCustomerId(int customerId)
        {
            var customer = await _customerRepository.GetByIdBasicAsync(customerId);
            if (customer == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy khách hàng."));

            var org = await _organizationService.GetByIdAsync(customer.Organization_information_id);

            var viewDto = new CustomerOrganizationViewDto
            {
                Customer_id = customerId,
                Organization_information_id = customer.Organization_information_id,
                Organization_code = org?.Organization_code ?? string.Empty,
                Organization_name = org?.Organization_name ?? string.Empty,
                Organization_address = org?.Address ?? string.Empty,
                Tax_number = org?.Tax_number ?? string.Empty,
                Recipient_name = customer.Recipient_name,
                Recipient_phone = customer.Recipient_phone,
                Recipient_address = customer.Recipient_address
            };
            return Ok(ApiResponse<CustomerOrganizationViewDto>.Ok(viewDto));
        }

        [HttpPut("organization")]
        public async Task<IActionResult> UpdateOrganization([FromBody] UpdateOrganizationByCustomerDto model)
        {
            var result = await _customerService.UpdateOrganizationAsync(model);
            if (result)
                return Ok(ApiResponse<bool>.Ok(true, "Cập nhật thành công."));
            return BadRequest(ApiResponse<bool>.Fail("Không thể cập nhật thông tin đơn vị."));
        }
    }
}