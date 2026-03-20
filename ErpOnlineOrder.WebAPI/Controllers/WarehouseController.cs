using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.WarehouseDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Application.DTOs;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ApiController
    {
        private readonly IWarehouseService _warehouseService;
        private readonly IPermissionService _permissionService;

        public WarehouseController(IWarehouseService warehouseService, IPermissionService permissionService)
        {
            _warehouseService = warehouseService;
            _permissionService = permissionService;
        }

        [HttpGet("for-select")]
        public async Task<IActionResult> GetForSelect()
        {
            var list = await _warehouseService.GetForSelectAsync();
            return Ok(ApiResponse<object>.Ok(list));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _warehouseService.GetAllAsync();
            var dtos = list.Select(EntityMappers.ToWarehouseDto).ToList();
            var userId = TryGetCurrentUserId();
            if (userId.HasValue && userId.Value > 0)
            {
                var permissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in dtos)
                    RecordPermissionEnricher.Enrich(dto, permissions, PermissionCodes.WarehouseUpdate, PermissionCodes.WarehouseDelete);
            }
            return Ok(ApiResponse<object>.Ok(dtos));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _warehouseService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy kho hàng."));
            var dto = EntityMappers.ToWarehouseDto(item);
            var userId = TryGetCurrentUserId();
            if (userId.HasValue && userId.Value > 0)
                await RecordPermissionEnricher.EnrichAsync(dto, userId.Value, _permissionService, PermissionCodes.WarehouseUpdate, PermissionCodes.WarehouseDelete);
            return Ok(ApiResponse<object>.Ok(dto));
        }

        [HttpGet("province/{provinceId}")]
        public async Task<IActionResult> GetByProvinceId(int provinceId)
        {
            var list = await _warehouseService.GetByProvinceIdAsync(provinceId);
            var dtos = list.Select(EntityMappers.ToWarehouseDto).ToList();
            var userId = TryGetCurrentUserId();
            if (userId.HasValue && userId.Value > 0)
            {
                var permissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in dtos)
                    RecordPermissionEnricher.Enrich(dto, permissions, PermissionCodes.WarehouseUpdate, PermissionCodes.WarehouseDelete);
            }
            return Ok(ApiResponse<object>.Ok(dtos));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto)
        {
            try
            {
                var created = await _warehouseService.CreateAsync(dto, GetCurrentUserId());
                return Ok(ApiResponse<object>.Ok(EntityMappers.ToWarehouseDto(created)));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateWarehouseDto dto)
        {
            if (id != dto.Id) return BadRequest(ApiResponse<object>.Fail("ID không khớp."));
            try
            {
                var result = await _warehouseService.UpdateAsync(dto, GetCurrentUserId());
                return Ok(ApiResponse<object>.Ok(new { success = result }));
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
                var result = await _warehouseService.DeleteAsync(id);
                return Ok(ApiResponse<object>.Ok(new { success = result }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
