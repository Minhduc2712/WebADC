using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.WebAPI.Attributes;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ApiController
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

        /// <summary>
        /// Lấy danh sách sản phẩm cho màn hình tạo/sửa đơn hàng, gán sản phẩm cho khách hàng.
        /// Cho phép user có ProductView, OrderCreate, OrderUpdate, CustomerProductView hoặc CustomerProductAssign.
        /// </summary>
        [HttpGet("for-order")]
        [RequireAnyPermission(PermissionCodes.ProductView, PermissionCodes.OrderCreate, PermissionCodes.OrderUpdate, PermissionCodes.CustomerProductView, PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> GetProductsForOrder()
        {
            var products = await _productService.SearchAsync(null, null, null);
            return Ok(products);
        }
        [HttpGet("{id:int}/entity")]
        [RequirePermission(PermissionCodes.ProductView)]
        public async Task<IActionResult> GetProductEntity(int id)
        {
            var product = await _productService.GetEntityByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }
        [HttpGet("{id:int}")]
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
                var created = await _productService.CreateProductAsync(dto, GetCurrentUserId());
                return Ok(created);
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
                var result = await _productService.UpdateProductAsync(id, dto, GetCurrentUserId());
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
                return BadRequest(new { message = "File không hợp lệ" });
            }
            return Ok(new { message = "Import successful", fileName = file.FileName, size = file.Length });
        }
        [HttpGet("export")]
        [RequirePermission(PermissionCodes.ProductExport)]
        public async Task<IActionResult> ExportProducts()
        {
            return Ok(new { message = "Export successful" });
        }
    }
}
