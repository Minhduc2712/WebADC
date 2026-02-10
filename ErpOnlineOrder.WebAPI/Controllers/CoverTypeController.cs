using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.CoverTypeDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoverTypeController : ControllerBase
    {
        private readonly ICoverTypeService _coverTypeService;

        public CoverTypeController(ICoverTypeService coverTypeService)
        {
            _coverTypeService = coverTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _coverTypeService.GetAllAsync();
            return Ok(list.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _coverTypeService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(MapToDto(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCoverTypeDto dto)
        {
            try
            {
                var entity = new Cover_type
                {
                    Cover_type_code = dto.Cover_type_code,
                    Cover_type_name = dto.Cover_type_name,
                    Created_by = 0,
                    Updated_by = 0
                };
                var created = await _coverTypeService.CreateAsync(entity);
                return Ok(MapToDto(created));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCoverTypeDto dto)
        {
            if (id != dto.Id) return BadRequest();
            try
            {
                var entity = new Cover_type
                {
                    Id = dto.Id,
                    Cover_type_code = dto.Cover_type_code,
                    Cover_type_name = dto.Cover_type_name,
                    Updated_by = 0
                };
                var result = await _coverTypeService.UpdateAsync(entity);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static CoverTypeDto MapToDto(Cover_type c)
        {
            return new CoverTypeDto
            {
                Id = c.Id,
                Cover_type_code = c.Cover_type_code,
                Cover_type_name = c.Cover_type_name,
                Created_at = c.Created_at,
                Updated_at = c.Updated_at
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _coverTypeService.DeleteAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
