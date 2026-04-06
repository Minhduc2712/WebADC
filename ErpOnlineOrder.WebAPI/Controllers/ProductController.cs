using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.WebAPI.Attributes;
using ErpOnlineOrder.Application.DTOs;

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
        [HttpGet("for-select")]
        [RequireAnyPermission(PermissionCodes.ProductView, PermissionCodes.OrderCreate, PermissionCodes.OrderUpdate, PermissionCodes.CustomerProductView, PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> GetForSelect()
        {
            var list = await _productService.GetForSelectAsync();
            return Ok(ApiResponse<object>.Ok(list));
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.ProductView)]
        public async Task<IActionResult> GetProducts([FromQuery] string? search)
        {
            var products = await _productService.SearchByAllAsync(search, TryGetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(products));
        }

        [HttpGet("paged")]
        [RequirePermission(PermissionCodes.ProductView)]
        public async Task<IActionResult> GetProductsPaged([FromQuery] ProductFilterRequest request)
        {
            var result = await _productService.GetAllPagedAsync(request, TryGetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("for-order")]
        [RequireAnyPermission(PermissionCodes.ProductView, PermissionCodes.OrderCreate, PermissionCodes.OrderUpdate, PermissionCodes.CustomerProductView, PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> GetProductsForOrder()
        {
            var products = await _productService.SearchAsync(null, null, null, TryGetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(products));
        }
        [HttpGet("{id:int}/entity")]
        [RequirePermission(PermissionCodes.ProductView)]
        public async Task<IActionResult> GetProductEntity(int id)
        {
            var product = await _productService.GetEntityByIdAsync(id);
            if (product == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy sản phẩm."));
            return Ok(ApiResponse<object>.Ok(product));
        }
        [HttpGet("{id:int}")]
        [RequirePermission(PermissionCodes.ProductView)]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetByIdAsync(id, TryGetCurrentUserId());
            if (product == null)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy sản phẩm."));
            }
            return Ok(ApiResponse<object>.Ok(product));
        }
        [HttpPost]
        [RequirePermission(PermissionCodes.ProductCreate)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            try
            {
                var created = await _productService.CreateProductAsync(dto, GetCurrentUserId());
                return Ok(ApiResponse<object>.Ok(created));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.ProductUpdate)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            if (id != dto.Id) return BadRequest(ApiResponse<object>.Fail("ID không khớp."));

            try
            {
                var result = await _productService.UpdateProductAsync(id, dto, GetCurrentUserId());
                return Ok(ApiResponse<object>.Ok(new { success = result }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.ProductDelete)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                return Ok(ApiResponse<object>.Ok(new { success = result }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [RequirePermission(PermissionCodes.ProductImport)]
        public async Task<IActionResult> ImportProducts(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.Fail("File không hợp lệ"));

            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return BadRequest(ApiResponse<object>.Fail("Chỉ chấp nhận file Excel (.xlsx, .xls)"));

            var userId = HttpContext.Session.GetInt32("UserId") ?? 1;
            using var stream = file.OpenReadStream();
            var result = await _productService.ImportProductsFromExcelAsync(stream, userId);
            return Ok(ApiResponse<object>.Ok(result, $"Nhập thành công {result.Succeeded}/{result.TotalRows} dòng"));
        }

        [HttpGet("import/template")]
        [RequirePermission(PermissionCodes.ProductImport)]
        public async Task<IActionResult> DownloadImportTemplate()
        {
            var bytes = await _productService.GetImportTemplateAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MauImportSanPham.xlsx");
        }

        [HttpGet("export")]
        [RequirePermission(PermissionCodes.ProductExport)]
        public async Task<IActionResult> ExportProducts([FromQuery] string? search)
        {
            var bytes = await _productService.ExportProductsToExcelAsync(search);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"SanPham_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        [HttpGet("shop")]
        public async Task<IActionResult> GetProductsForShop([FromQuery] ProductForShopFilterRequest request, [FromQuery] int? customerId)
        {
            var paged = await _productService.GetProductsForShopPagedAsync(customerId, request);
            return Ok(ApiResponse<PagedResult<ProductDTO>>.Ok(paged));
        }

        [HttpGet("shop/categories")]
        public async Task<IActionResult> GetCategoriesForShop([FromQuery] int? customerId)
        {
            var categories = await _productService.GetCategoriesForShopAsync(customerId);
            return Ok(ApiResponse<IEnumerable<string>>.Ok(categories));
        }

        [HttpGet("shop/publishers")]
        public async Task<IActionResult> GetPublishersForShop([FromQuery] int? customerId)
        {
            var publishers = await _productService.GetPublishersForShopAsync(customerId);
            return Ok(ApiResponse<IEnumerable<string>>.Ok(publishers));
        }

        [HttpGet("shop/authors")]
        public async Task<IActionResult> GetAuthorsForShop([FromQuery] int? customerId)
        {
            var authors = await _productService.GetAuthorsForShopAsync(customerId);
            return Ok(ApiResponse<IEnumerable<string>>.Ok(authors));
        }

        [HttpGet("{productId}/check-assigned")]
        public async Task<IActionResult> CheckAssigned(int productId, [FromQuery] int customerId)
        {
            var hasAccess = await _productService.IsProductAssignedToCustomerAsync(productId, customerId);
            return Ok(ApiResponse<object>.Ok(new { Assigned = hasAccess }));
        }

        [HttpGet("shop/{productId}/related")]
        public async Task<IActionResult> GetRelatedProducts(int productId, [FromQuery] int? customerId, [FromQuery] int limit = 4)
        {
            var product = await _productService.GetByIdAsync(productId);
            if (product == null) return NotFound(ApiResponse<IEnumerable<ProductDTO>>.Fail("Không tìm thấy SP."));

            var related = await _productService.GetRelatedProductsForShopAsync(productId, product.Categories, customerId, limit);
            return Ok(ApiResponse<IEnumerable<ProductDTO>>.Ok(related));
        }

        [HttpGet("shop/{productId}/detail")]
        public async Task<IActionResult> GetProductForShop(int productId, [FromQuery] int? customerId)
        {
            var product = await _productService.GetByIdAsync(productId);
            if (product == null) return NotFound(ApiResponse<ProductDTO>.Fail("Không tìm thấy sản phẩm."));
            return Ok(ApiResponse<ProductDTO>.Ok(product));
        }
    }
}
