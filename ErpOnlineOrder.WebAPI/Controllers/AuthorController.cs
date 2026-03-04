using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.AuthorDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorController : ApiController
    {
        private readonly IAuthorService _authorService;

        public AuthorController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _authorService.GetAllAsync();
            return Ok(list.Select(EntityMappers.ToAuthorDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _authorService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(EntityMappers.ToAuthorDto(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAuthorDto dto)
        {
            try
            {
                var created = await _authorService.CreateAsync(dto, GetCurrentUserId());
                return Ok(EntityMappers.ToAuthorDto(created));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAuthorDto dto)
        {
            if (id != dto.Id) return BadRequest();
            try
            {
                var result = await _authorService.UpdateAsync(id, dto, GetCurrentUserId());
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
                var result = await _authorService.DeleteAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
