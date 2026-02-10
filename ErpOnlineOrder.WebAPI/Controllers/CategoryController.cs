using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(MapToDto(category));
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            try
            {
                var entity = new Category
                {
                    Category_code = dto.Category_code,
                    Category_name = dto.Category_name,
                    Created_by = 0,
                    Updated_by = 0
                };
                var created = await _categoryService.CreateCategoryAsync(entity);
                return Ok(MapToDto(created));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (id != dto.Id) return BadRequest();

            try
            {
                var entity = new Category
                {
                    Id = dto.Id,
                    Category_code = dto.Category_code,
                    Category_name = dto.Category_name,
                    Updated_by = 0
                };
                var result = await _categoryService.UpdateCategoryAsync(entity);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static CategoryDTO MapToDto(Category c)
        {
            return new CategoryDTO
            {
                Id = c.Id,
                Category_code = c.Category_code,
                Category_name = c.Category_name,
                Created_at = c.Created_at,
                Updated_at = c.Updated_at
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            // Require permission: DELETE_CATEGORY
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}