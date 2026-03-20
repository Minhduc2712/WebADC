using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Constants;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseExportController : ApiController
    {
        private readonly IWarehouseExportService _exportService;
        private readonly ICustomerRepository _customerRepository;

        public WarehouseExportController(IWarehouseExportService exportService, ICustomerRepository customerRepository)
        {
            _exportService = exportService;
            _customerRepository = customerRepository;
        }

        #region CRUD cơ bản
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var exports = await _exportService.GetAllAsync(TryGetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(exports));
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetAllPaged([FromQuery] WarehouseExportFilterRequest request)
        {
            var result = await _exportService.GetAllPagedAsync(request, TryGetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(result));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var export = await _exportService.GetByIdAsync(id, TryGetCurrentUserId());
            if (export == null)
            {
                return NotFound(ApiResponse<object>.Fail("Phiếu xuất kho không tồn tại"));
            }
            return Ok(ApiResponse<object>.Ok(export));
        }
        [HttpGet("invoice/{invoiceId}")]
        public async Task<IActionResult> GetByInvoice(int invoiceId)
        {
            var exports = await _exportService.GetByInvoiceIdAsync(invoiceId);
            return Ok(ApiResponse<object>.Ok(exports));
        }
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var exports = await _exportService.GetByCustomerIdAsync(customerId);
            return Ok(ApiResponse<object>.Ok(exports));
        }
        [HttpGet("my-exports")]
        public async Task<IActionResult> GetMyExports()
        {
            var userId = TryGetCurrentUserId();
            if (!userId.HasValue || userId.Value <= 0)
                return Unauthorized(ApiResponse<object>.Fail("Vui lòng đăng nhập"));

            var customer = await _customerRepository.GetByUserIdAsync(userId.Value);
            if (customer == null)
                return Ok(ApiResponse<object>.Ok(new List<WarehouseExportDto>()));

            var exports = await _exportService.GetByCustomerIdAsync(customer.Id);
            return Ok(ApiResponse<object>.Ok(exports));
        }
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouse(int warehouseId)
        {
            var exports = await _exportService.GetByWarehouseIdAsync(warehouseId);
            return Ok(ApiResponse<object>.Ok(exports));
        }
        [HttpPost]
        public async Task<IActionResult> CreateExport([FromBody] CreateWarehouseExportDto dto)
        {
            try
            {
                var result = await _exportService.CreateExportFromInvoiceAsync(dto, GetCurrentUserId());
                if (result == null)
                {
                    return BadRequest(ApiResponse<object>.Fail("Không thể tạo phiếu xuất kho. Dữ liệu không hợp lệ hoặc chưa đủ điều kiện xuất kho."));
                }
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<object>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExport(int id, [FromBody] UpdateWarehouseExportDto dto)
        {
            try
            {
                var result = await _exportService.UpdateExportAsync(id, dto, GetCurrentUserId());
                if (!result)
                {
                    return NotFound(ApiResponse<object>.Fail("Phiếu xuất kho không tồn tại"));
                }
                return Ok(ApiResponse<object>.Ok(null, "Đã cập nhật phiếu xuất kho"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPatch("{id}/delivery-status")]
        public async Task<IActionResult> UpdateDeliveryStatus(int id, [FromQuery] string status)
        {
            try
            {
                var result = await _exportService.UpdateDeliveryStatusAsync(id, status, GetCurrentUserId());
                if (!result)
                {
                    return NotFound(ApiResponse<object>.Fail("Phiếu xuất kho không tồn tại"));
                }
                return Ok(ApiResponse<object>.Ok(null, $"Đã cập nhật trạng thái giao hàng thành: {status}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmExport(int id)
        {
            try
            {
                var result = await _exportService.ConfirmExportAsync(id, GetCurrentUserId());
                if (!result)
                {
                    return NotFound(ApiResponse<object>.Fail("Phiếu xuất kho không tồn tại"));
                }
                return Ok(ApiResponse<object>.Ok(null, "Đã xác nhận phiếu xuất kho"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
        {
            try
            {
                var result = await _exportService.UpdateStatusAsync(id, status, GetCurrentUserId());
                if (!result)
                    return NotFound(ApiResponse<object>.Fail("Phiếu xuất kho không tồn tại"));
                return Ok(ApiResponse<object>.Ok(null, $"Đã cập nhật trạng thái phiếu xuất kho thành: {ExportStatuses.ToDisplayText(status)}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelExport(int id)
        {
            try
            {
                var result = await _exportService.CancelExportAsync(id, GetCurrentUserId());
                if (!result)
                {
                    return NotFound(ApiResponse<object>.Fail("Phiếu xuất kho không tồn tại"));
                }
                return Ok(ApiResponse<object>.Ok(null, "Đã hủy phiếu xuất kho"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExport(int id)
        {
            var result = await _exportService.DeleteExportAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.Fail("Phiếu xuất kho không tồn tại"));
            }
            return Ok(ApiResponse<object>.Ok(null, "Đã xóa phiếu xuất kho"));
        }

        #endregion

        #region Tách/Gộp phiếu xuất kho
        [HttpPost("split")]
        public async Task<IActionResult> SplitExport([FromBody] SplitWarehouseExportDto dto)
        {
            try
            {
                var result = await _exportService.SplitExportAsync(dto, GetCurrentUserId());
                if (!result.Success)
                {
                    return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Lỗi khi tách phiếu"));
                }
                return Ok(ApiResponse<object>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPost("merge")]
        public async Task<IActionResult> MergeExports([FromBody] MergeWarehouseExportsDto dto)
        {
            try
            {
                var result = await _exportService.MergeExportsAsync(dto, GetCurrentUserId());
                if (!result.Success)
                {
                    return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Lỗi khi gộp phiếu"));
                }
                return Ok(ApiResponse<object>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPost("{id}/undo-split")]
        public async Task<IActionResult> UndoSplit(int id)
        {
            try
            {
                var result = await _exportService.UndoSplitAsync(id, GetCurrentUserId());
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Không thể hoàn tác tách phiếu xuất kho. Phiếu không tồn tại hoặc không ở trạng thái Đã tách."));
                }
                return Ok(ApiResponse<object>.Ok(null, "Đã hoàn tác tách phiếu xuất kho"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPost("{id}/undo-merge")]
        public async Task<IActionResult> UndoMerge(int id)
        {
            try
            {
                var result = await _exportService.UndoMergeAsync(id, GetCurrentUserId());
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Không thể hoàn tác gộp phiếu xuất kho. Phiếu không tồn tại hoặc không ở trạng thái đã gộp hợp lệ."));
                }
                return Ok(ApiResponse<object>.Ok(null, "Đã hoàn tác gộp phiếu xuất kho"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpGet("{id}/children")]
        public async Task<IActionResult> GetChildExports(int id)
        {
            var childExports = await _exportService.GetChildExportsAsync(id);
            return Ok(ApiResponse<object>.Ok(childExports));
        }
        [HttpGet("check-invoice/{invoiceId}")]
        public async Task<IActionResult> CheckInvoiceHasExport(int invoiceId)
        {
            var hasExport = await _exportService.HasExportForInvoiceAsync(invoiceId);
            return Ok(ApiResponse<object>.Ok(new { invoice_id = invoiceId, has_export = hasExport }));
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportExcel([FromQuery] string? status)
        {
            var bytes = await _exportService.ExportWarehouseExportsToExcelAsync(status);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"PhieuXuatKho_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        #endregion

        [HttpGet("customer/{customerId}/paged")]
        public async Task<IActionResult> GetByCustomerIdPaged(int customerId, [FromQuery] WarehouseExportFilterRequest request)
        {
            var paged = await _exportService.GetByCustomerIdPagedAsync(customerId, request);
            return Ok(ApiResponse<PagedResult<WarehouseExportDto>>.Ok(paged));
        }
    }
}
