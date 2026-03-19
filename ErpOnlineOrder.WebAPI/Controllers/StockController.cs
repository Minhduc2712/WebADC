using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.StockDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ApiController
    {
        private readonly IStockService _stockService;
        private readonly IPermissionService _permissionService;

        public StockController(IStockService stockService, IPermissionService permissionService)
        {
            _stockService = stockService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? warehouseId = null, [FromQuery] string? searchTerm = null)
        {
            var list = await _stockService.GetAllAsync(warehouseId, searchTerm);
            var dtos = list.Select(EntityMappers.ToStockDto).ToList();

            var userId = TryGetCurrentUserId();
            if (userId.HasValue && userId.Value > 0)
            {
                var permissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in dtos)
                    RecordPermissionEnricher.Enrich(dto, permissions, PermissionCodes.StockUpdate, PermissionCodes.StockDelete);
            }

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var stock = await _stockService.GetByIdAsync(id);
            if (stock == null) return NotFound(new { message = "Không tìm thấy bản ghi tồn kho." });

            var dto = EntityMappers.ToStockDto(stock);
            var userId = TryGetCurrentUserId();
            if (userId.HasValue && userId.Value > 0)
                await RecordPermissionEnricher.EnrichAsync(dto, userId.Value, _permissionService, PermissionCodes.StockUpdate, PermissionCodes.StockDelete);

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockDto dto)
        {
            try
            {
                var stock = await _stockService.CreateAsync(dto, GetCurrentUserId());
                var resultDto = EntityMappers.ToStockDto(stock);
                return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStockDto dto)
        {
            if (id != dto.Id) return BadRequest(new { message = "ID trên URL không khớp với ID dữ liệu cập nhật tồn kho." });

            try
            {
                var result = await _stockService.UpdateQuantityAsync(id, dto.Quantity, GetCurrentUserId());
                if (!result)
                    return NotFound(new { message = "Không tìm thấy bản ghi tồn kho cần cập nhật." });

                return Ok(new { success = true, message = "Đã cập nhật tồn kho thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
