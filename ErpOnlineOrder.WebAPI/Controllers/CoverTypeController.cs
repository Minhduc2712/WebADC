using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.CoverTypeDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoverTypeController : ApiController
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
            return Ok(list.Select(EntityMappers.ToCoverTypeDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _coverTypeService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(EntityMappers.ToCoverTypeDto(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCoverTypeDto dto)
        {
            try
            {
                var created = await _coverTypeService.CreateAsync(dto, GetCurrentUserId());
                return Ok(EntityMappers.ToCoverTypeDto(created));
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
                var result = await _coverTypeService.UpdateAsync(id, dto, GetCurrentUserId());
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
