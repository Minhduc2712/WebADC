using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ApiController
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
            return Ok(ApiResponse<object>.Ok(categories.Select(EntityMappers.ToCategoryDto)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy danh mục."));
            return Ok(ApiResponse<object>.Ok(EntityMappers.ToCategoryDto(category)));
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            try
            {
                var created = await _categoryService.CreateCategoryAsync(dto, GetCurrentUserId());
                return Ok(ApiResponse<object>.Ok(EntityMappers.ToCategoryDto(created)));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (id != dto.Id) return BadRequest(ApiResponse<object>.Fail("ID không khớp."));

            try
            {
                var result = await _categoryService.UpdateCategoryAsync(id, dto, GetCurrentUserId());
                return Ok(ApiResponse<object>.Ok(new { success = result }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            // Require permission: DELETE_CATEGORY
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                return Ok(ApiResponse<object>.Ok(new { success = result }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}