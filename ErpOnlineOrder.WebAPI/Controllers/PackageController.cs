using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.PackageDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : ApiController
    {
        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _packageService.GetAllAsync();
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _packageService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy gói."));
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("by-organization/{organizationId}")]
        public async Task<IActionResult> GetByOrganization(int organizationId)
        {
            var result = await _packageService.GetByOrganizationAsync(organizationId);
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("by-region/{regionId}")]
        public async Task<IActionResult> GetByRegion(int regionId)
        {
            var result = await _packageService.GetByRegionAsync(regionId);
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("by-province/{provinceId}")]
        public async Task<IActionResult> GetByProvince(int provinceId)
        {
            var result = await _packageService.GetByProvinceAsync(provinceId);
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("by-ward/{wardId}")]
        public async Task<IActionResult> GetByWard(int wardId)
        {
            var result = await _packageService.GetByWardAsync(wardId);
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePackageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

            var createdBy = GetCurrentUserId();
            try
            {
                var result = await _packageService.CreateAsync(dto, createdBy);
                return Ok(ApiResponse<object>.Ok(result));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePackageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

            var updatedBy = GetCurrentUserId();
            var success = await _packageService.UpdateAsync(id, dto, updatedBy);
            if (!success)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy gói."));

            return Ok(ApiResponse<object>.Ok("Cập nhật thành công."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _packageService.DeleteAsync(id);
            if (!success)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy gói."));

            return Ok(ApiResponse<object>.Ok("Xóa thành công."));
        }

        [HttpPost("{id}/products")]
        public async Task<IActionResult> AddProduct(int id, [FromBody] CreatePackageProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

            var createdBy = GetCurrentUserId();
            var success = await _packageService.AddProductAsync(id, dto, createdBy);
            if (!success)
                return Conflict(ApiResponse<object>.Fail("Sản phẩm đã tồn tại trong gói."));

            return Ok(ApiResponse<object>.Ok("Thêm sản phẩm vào gói thành công."));
        }

        [HttpDelete("{id}/products/{productId}")]
        public async Task<IActionResult> RemoveProduct(int id, int productId)
        {
            var success = await _packageService.RemoveProductAsync(id, productId);
            if (!success)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy sản phẩm trong gói."));

            return Ok(ApiResponse<object>.Ok("Đã xóa sản phẩm khỏi gói."));
        }
    }
}
