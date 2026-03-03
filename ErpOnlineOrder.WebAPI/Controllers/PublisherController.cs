using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.PublisherDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;

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
            return Ok(list.Select(EntityMappers.ToPublisherDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _publisherService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(EntityMappers.ToPublisherDto(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePublisherDto dto)
        {
            try
            {
                var created = await _publisherService.CreateAsync(dto, GetCurrentUserId());
                return Ok(EntityMappers.ToPublisherDto(created));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePublisherDto dto)
        {
            if (id != dto.Id) return BadRequest();
            try
            {
                var result = await _publisherService.UpdateAsync(dto, GetCurrentUserId());
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
                var result = await _publisherService.DeleteAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
