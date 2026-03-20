using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.CoverTypeDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Application.DTOs;

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
            return Ok(ApiResponse<object>.Ok(list.Select(EntityMappers.ToCoverTypeDto)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _coverTypeService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy loại bìa."));
            return Ok(ApiResponse<object>.Ok(EntityMappers.ToCoverTypeDto(item)));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCoverTypeDto dto)
        {
            try
            {
                var created = await _coverTypeService.CreateAsync(dto, GetCurrentUserId());
                return Ok(ApiResponse<object>.Ok(EntityMappers.ToCoverTypeDto(created)));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCoverTypeDto dto)
        {
            if (id != dto.Id) return BadRequest(ApiResponse<object>.Fail("ID không khớp."));
            try
            {
                var result = await _coverTypeService.UpdateAsync(id, dto, GetCurrentUserId());
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
                var result = await _coverTypeService.DeleteAsync(id);
                return Ok(ApiResponse<object>.Ok(new { success = result }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
