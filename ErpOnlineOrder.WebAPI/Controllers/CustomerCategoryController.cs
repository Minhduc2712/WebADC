using ErpOnlineOrder.Application.DTOs.CustomerCategoryDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerCategoryController : ApiController
    {
        private readonly ICustomerCategoryService _customerCategoryService;

        public CustomerCategoryController(ICustomerCategoryService customerCategoryService)
        {
            _customerCategoryService = customerCategoryService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _customerCategoryService.GetAllAsync();
            return Ok(result);
        }
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomerId(int customerId)
        {
            var result = await _customerCategoryService.GetCategoriesByCustomerIdAsync(customerId);
            return Ok(result);
        }
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategoryId(int categoryId)
        {
            var result = await _customerCategoryService.GetCustomersByCategoryIdAsync(categoryId);
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _customerCategoryService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerCategoryDto dto)
        {
            var createdBy = GetCurrentUserId();
            
            var result = await _customerCategoryService.AddCategoryToCustomerAsync(dto, createdBy);
            if (result == null)
            {
                return BadRequest("Danh m?c dã du?c gán cho khách hàng này ho?c d? li?u không h?p l?.");
            }
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        [HttpPost("assign")]
        public async Task<IActionResult> AssignCategories([FromBody] AssignCategoriesToCustomerDto dto)
        {
            var result = await _customerCategoryService.AssignCategoriesToCustomerAsync(dto, GetCurrentUserId());
            if (!result)
            {
                return BadRequest("Không th? gán danh m?c cho khách hàng.");
            }
            return Ok(new { message = "Gán danh m?c thành công." });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerCategoryDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID không kh?p.");
            }

            int updatedBy = GetCurrentUserId();
            
            var result = await _customerCategoryService.UpdateCustomerCategoryAsync(dto, updatedBy);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int deletedBy = GetCurrentUserId();
            
            var result = await _customerCategoryService.RemoveCategoryFromCustomerAsync(id, deletedBy);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        [HttpGet("check/{customerId}/{categoryId}")]
        public async Task<IActionResult> CheckPermission(int customerId, int categoryId)
        {
            var canOrder = await _customerCategoryService.CanCustomerOrderFromCategoryAsync(customerId, categoryId);
            return Ok(new { canOrder });
        }
    }
}
