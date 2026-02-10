using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.WebAPI.Attributes;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        [HttpGet]
        [RequirePermission(PermissionCodes.ProductView)]
        public async Task<IActionResult> GetProducts([FromQuery] string? name, [FromQuery] string? author, [FromQuery] string? publisher)
        {
            var products = await _productService.SearchAsync(name, author, publisher);
            return Ok(products);
        }
        [HttpGet("{id}/entity")]
        [RequirePermission(PermissionCodes.ProductView)]
        public async Task<IActionResult> GetProductEntity(int id)
        {
            var product = await _productService.GetEntityByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.ProductView)]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        [HttpPost]
        [RequirePermission(PermissionCodes.ProductCreate)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            try
            {
                var now = DateTime.UtcNow;
                var entity = new Product
                {
                    Product_code = dto.Product_code,
                    Product_name = dto.Product_name,
                    Product_price = dto.Product_price,
                    Product_link = dto.Product_link,
                    Product_description = dto.Product_description,
                    Tax_rate = dto.Tax_rate,
                    Cover_type_id = dto.Cover_type_id,
                    Publisher_id = dto.Publisher_id,
                    Distributor_id = dto.Distributor_id,
                    Created_by = 0,
                    Updated_by = 0,
                    Product_Categories = dto.CategoryIds?.Select(cid => new Product_category
                    {
                        Category_id = cid,
                        Created_by = 0,
                        Updated_by = 0,
                        Created_at = now,
                        Updated_at = now,
                        Is_deleted = false
                    }).ToList() ?? new List<Product_category>(),
                    Product_Authors = dto.AuthorIds?.Select(aid => new Product_author
                    {
                        Author_id = aid,
                        Created_by = 0,
                        Updated_by = 0,
                        Created_at = now,
                        Updated_at = now,
                        Is_deleted = false
                    }).ToList() ?? new List<Product_author>()
                };
                var created = await _productService.CreateProductAsync(entity);
                var createdDto = await _productService.GetByIdAsync(created.Id);
                return Ok(createdDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.ProductUpdate)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            if (id != dto.Id) return BadRequest();

            try
            {
                var now = DateTime.UtcNow;
                var entity = new Product
                {
                    Id = dto.Id,
                    Product_code = dto.Product_code,
                    Product_name = dto.Product_name,
                    Product_price = dto.Product_price,
                    Product_link = dto.Product_link,
                    Product_description = dto.Product_description,
                    Tax_rate = dto.Tax_rate,
                    Cover_type_id = dto.Cover_type_id,
                    Publisher_id = dto.Publisher_id,
                    Distributor_id = dto.Distributor_id,
                    Updated_by = 0,
                    Product_Categories = dto.CategoryIds?.Select(cid => new Product_category
                    {
                        Product_id = id,
                        Category_id = cid,
                        Created_by = 0,
                        Updated_by = 0,
                        Created_at = now,
                        Updated_at = now,
                        Is_deleted = false
                    }).ToList() ?? new List<Product_category>(),
                    Product_Authors = dto.AuthorIds?.Select(aid => new Product_author
                    {
                        Product_id = id,
                        Author_id = aid,
                        Created_by = 0,
                        Updated_by = 0,
                        Created_at = now,
                        Updated_at = now,
                        Is_deleted = false
                    }).ToList() ?? new List<Product_author>()
                };
                var result = await _productService.UpdateProductAsync(entity);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.ProductDelete)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [RequirePermission(PermissionCodes.ProductImport)]
        public async Task<IActionResult> ImportProducts(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "File kh?ng h?p l?" });
            }

            // TODO: Implement import logic
            return Ok(new { message = "Import successful", fileName = file.FileName, size = file.Length });
        }
        [HttpGet("export")]
        [RequirePermission(PermissionCodes.ProductExport)]
        public async Task<IActionResult> ExportProducts()
        {
            // TODO: Implement export logic
            return Ok(new { message = "Export successful" });
        }
    }
}