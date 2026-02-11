using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.WarehouseDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ApiController
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _warehouseService.GetAllAsync();
            return Ok(list.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _warehouseService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(MapToDto(item));
        }

        [HttpGet("province/{provinceId}")]
        public async Task<IActionResult> GetByProvinceId(int provinceId)
        {
            var list = await _warehouseService.GetByProvinceIdAsync(provinceId);
            return Ok(list.Select(MapToDto));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto)
        {
            try
            {
                var created = await _warehouseService.CreateAsync(dto, GetCurrentUserId());
                return Ok(MapToDto(created));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateWarehouseDto dto)
        {
            if (id != dto.Id) return BadRequest();
            try
            {
                var result = await _warehouseService.UpdateAsync(dto, GetCurrentUserId());
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static WarehouseDto MapToDto(Warehouse w)
        {
            return new WarehouseDto
            {
                Id = w.Id,
                Warehouse_code = w.Warehouse_code,
                Warehouse_name = w.Warehouse_name,
                Warehouse_address = w.Warehouse_address,
                Province_id = w.Province_id,
                Province_name = w.Province?.Province_name,
                Created_at = w.Created_at,
                Updated_at = w.Updated_at
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _warehouseService.DeleteAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
