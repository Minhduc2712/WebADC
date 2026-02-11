using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseExportController : ApiController
    {
        private readonly IWarehouseExportService _exportService;

        public WarehouseExportController(IWarehouseExportService exportService)
        {
            _exportService = exportService;
        }

        #region CRUD c? b?n
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var exports = await _exportService.GetAllAsync();
            return Ok(exports);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var export = await _exportService.GetByIdAsync(id);
            if (export == null)
            {
                return NotFound(new { message = "Phi?u xu?t kho không t?n t?i" });
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
                    return BadRequest(new { message = "Không th? t?o phi?u xu?t kho" });
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
                    return NotFound(new { message = "Phi?u xu?t kho không t?n t?i" });
                }
                return Ok(new { success = true, message = $"?ã c?p nh?t tr?ng thái giao hàng thành: {status}" });
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
                    return NotFound(new { message = "Phi?u xu?t kho không t?n t?i" });
                }
                return Ok(new { success = true, message = "?ã xác nh?n phi?u xu?t kho" });
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
                    return NotFound(new { message = "Phi?u xu?t kho không t?n t?i" });
                }
                return Ok(new { success = true, message = "?ã h?y phi?u xu?t kho" });
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
                return NotFound(new { message = "Phi?u xu?t kho không t?n t?i" });
            }
            return Ok(new { success = true, message = "?ã xóa phi?u xu?t kho" });
        }

        #endregion

        #region Tách/G?p phi?u xu?t kho
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
                    return BadRequest(new { message = "Không th? hoàn tác" });
                }
                return Ok(new { success = true, message = "?ã hoàn tác tách phi?u xu?t kho" });
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
                    return BadRequest(new { message = "Không th? hoàn tác" });
                }
                return Ok(new { success = true, message = "?ã hoàn tác g?p phi?u xu?t kho" });
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

        #endregion
    }
}
