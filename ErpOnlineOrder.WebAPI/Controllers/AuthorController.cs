using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.AuthorDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorController : ControllerBase
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
            return Ok(list.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _authorService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(MapToDto(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAuthorDto dto)
        {
            try
            {
                var entity = new Author
                {
                    Author_code = dto.Author_code,
                    Author_name = dto.Author_name,
                    Pen_name = dto.Pen_name,
                    Email_author = dto.Email_author,
                    Phone_number = dto.Phone_number,
                    birth_date = dto.birth_date,
                    death_date = dto.death_date,
                    Nationality = dto.Nationality,
                    Biography = dto.Biography,
                    Created_by = 0,
                    Updated_by = 0
                };
                var created = await _authorService.CreateAsync(entity);
                return Ok(MapToDto(created));
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
                var entity = new Author
                {
                    Id = dto.Id,
                    Author_code = dto.Author_code,
                    Author_name = dto.Author_name,
                    Pen_name = dto.Pen_name,
                    Email_author = dto.Email_author,
                    Phone_number = dto.Phone_number,
                    birth_date = dto.birth_date,
                    death_date = dto.death_date,
                    Nationality = dto.Nationality,
                    Biography = dto.Biography,
                    Updated_by = 0
                };
                var result = await _authorService.UpdateAsync(entity);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static AuthorDto MapToDto(Author a)
        {
            return new AuthorDto
            {
                Id = a.Id,
                Author_code = a.Author_code,
                Author_name = a.Author_name,
                Pen_name = a.Pen_name,
                Email_author = a.Email_author,
                Phone_number = a.Phone_number,
                birth_date = a.birth_date,
                death_date = a.death_date,
                Nationality = a.Nationality,
                Biography = a.Biography,
                Created_at = a.Created_at,
                Updated_at = a.Updated_at
            };
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
