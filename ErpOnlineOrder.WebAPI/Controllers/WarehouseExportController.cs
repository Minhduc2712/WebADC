using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
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
            return Ok(exports);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetAllPaged([FromQuery] WarehouseExportFilterRequest request)
        {
            var result = await _exportService.GetAllPagedAsync(request, TryGetCurrentUserId());
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var export = await _exportService.GetByIdAsync(id, TryGetCurrentUserId());
            if (export == null)
            {
                return NotFound(new { message = "Phiếu xuất kho không tồn tại" });
            }
            return Ok(export);
        }
        [HttpGet("invoice/{invoiceId}")]
        public async Task<IActionResult> GetByInvoice(int invoiceId)
        {
            var exports = await _exportService.GetByInvoiceIdAsync(invoiceId);
            return Ok(exports);
        }
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var exports = await _exportService.GetByCustomerIdAsync(customerId);
            return Ok(exports);
        }
        /// <summary>Lấy phiếu xuất kho của khách hàng đang đăng nhập (dùng UserId từ token/session).</summary>
        [HttpGet("my-exports")]
        public async Task<IActionResult> GetMyExports()
        {
            var userId = TryGetCurrentUserId();
            if (!userId.HasValue || userId.Value <= 0)
                return Unauthorized(new { message = "Vui lòng đăng nhập" });

            var customer = await _customerRepository.GetByUserIdAsync(userId.Value);
            if (customer == null)
                return Ok(new List<WarehouseExportDto>());

            var exports = await _exportService.GetByCustomerIdAsync(customer.Id);
            return Ok(exports);
        }
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouse(int warehouseId)
        {
            var exports = await _exportService.GetByWarehouseIdAsync(warehouseId);
            return Ok(exports);
        }
        [HttpPost]
        public async Task<IActionResult> CreateExport([FromBody] CreateWarehouseExportDto dto)
        {
            try
            {
                var result = await _exportService.CreateExportFromInvoiceAsync(dto, GetCurrentUserId());
                if (result == null)
                {
                    return BadRequest(new { message = "Không thể tạo phiếu xuất kho" });
                }
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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
                    return NotFound(new { message = "Phiếu xuất kho không tồn tại" });
                }
                return Ok(new { success = true, message = $"Đã cập nhật trạng thái giao hàng thành: {status}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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
                    return NotFound(new { message = "Phiếu xuất kho không tồn tại" });
                }
                return Ok(new { success = true, message = "Đã xác nhận phiếu xuất kho" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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
                    return NotFound(new { message = "Phiếu xuất kho không tồn tại" });
                }
                return Ok(new { success = true, message = "Đã hủy phiếu xuất kho" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExport(int id)
        {
            var result = await _exportService.DeleteExportAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Phiếu xuất kho không tồn tại" });
            }
            return Ok(new { success = true, message = "Đã xóa phiếu xuất kho" });
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
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
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
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
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
                    return BadRequest(new { message = "Không thể hoàn tác" });
                }
                return Ok(new { success = true, message = "Đã hoàn tác tách phiếu xuất kho" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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
                    return BadRequest(new { message = "Không thể hoàn tác" });
                }
                return Ok(new { success = true, message = "Đã hoàn tác gộp phiếu xuất kho" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("{id}/children")]
        public async Task<IActionResult> GetChildExports(int id)
        {
            var childExports = await _exportService.GetChildExportsAsync(id);
            return Ok(childExports);
        }
        [HttpGet("check-invoice/{invoiceId}")]
        public async Task<IActionResult> CheckInvoiceHasExport(int invoiceId)
        {
            var hasExport = await _exportService.HasExportForInvoiceAsync(invoiceId);
            return Ok(new { invoice_id = invoiceId, has_export = hasExport });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportExcel([FromQuery] string? status)
        {
            var bytes = await _exportService.ExportWarehouseExportsToExcelAsync(status);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"PhieuXuatKho_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        #endregion
    }
}
