using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.PublisherDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Application.DTOs;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublisherController : ApiController
    {
        private readonly IPublisherService _publisherService;

        public PublisherController(IPublisherService publisherService)
        {
            _publisherService = publisherService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _publisherService.GetAllAsync();
            return Ok(ApiResponse<object>.Ok(list.Select(EntityMappers.ToPublisherDto)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _publisherService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy nhà xuất bản."));
            return Ok(ApiResponse<object>.Ok(EntityMappers.ToPublisherDto(item)));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePublisherDto dto)
        {
            try
            {
                var created = await _publisherService.CreateAsync(dto, GetCurrentUserId());
                return Ok(ApiResponse<object>.Ok(EntityMappers.ToPublisherDto(created)));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePublisherDto dto)
        {
            if (id != dto.Id) return BadRequest(ApiResponse<object>.Fail("ID không khớp."));
            try
            {
                var result = await _publisherService.UpdateAsync(dto, GetCurrentUserId());
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
                var result = await _publisherService.DeleteAsync(id);
                return Ok(ApiResponse<object>.Ok(new { success = result }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
