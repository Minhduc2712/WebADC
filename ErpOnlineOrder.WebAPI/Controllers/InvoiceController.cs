using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Constants;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebAPI.Attributes;
using ErpOnlineOrder.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ApiController
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _invoiceService.GetAllAsync(TryGetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(list));
        }

        [HttpGet("for-merge")]
        public async Task<IActionResult> GetForMerge()
        {
            var list = await _invoiceService.GetForMergeAsync(TryGetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(list));
        }

        [HttpGet("for-warehouse-export")]
        public async Task<IActionResult> GetForWarehouseExport()
        {
            var list = await _invoiceService.GetForWarehouseExportSelectAsync(TryGetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(list));
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetAllPaged([FromQuery] InvoiceFilterRequest request)
        {
            var result = await _invoiceService.GetAllPagedAsync(request, TryGetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id, TryGetCurrentUserId());
            if (invoice == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy hóa đơn."));
            return Ok(ApiResponse<object>.Ok(invoice));
        }

        [HttpPost("split")]
        public async Task<IActionResult> SplitInvoice([FromBody] SplitInvoiceDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _invoiceService.SplitInvoiceAsync(dto, userId);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Lỗi khi tách hóa đơn"));
            }
            return Ok(ApiResponse<object>.Ok(result));
        }
        [HttpPost("merge")]
        public async Task<IActionResult> MergeInvoices([FromBody] MergeInvoicesDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _invoiceService.MergeInvoicesAsync(dto, userId);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Lỗi khi gộp hóa đơn"));
            }
            return Ok(ApiResponse<object>.Ok(result));
        }
        [HttpPost("{id}/undo-split")]
        public async Task<IActionResult> UndoSplit(int id)
        {
            try
            {
                var userId = int.TryParse(User.FindFirst("UserId")?.Value, out int uid) ? uid : 0;
                var result = await _invoiceService.UndoSplitAsync(id, userId);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Không thể hoàn tác tách hóa đơn. Hóa đơn không tồn tại hoặc không ở trạng thái Đã tách."));
                }
                return Ok(ApiResponse<object>.Ok(null, "Đã hoàn tác tách hóa đơn"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPost("{id}/undo-merge")]
        public async Task<IActionResult> UndoMerge(int id)
        {
            var userId = int.TryParse(User.FindFirst("UserId")?.Value, out int uid) ? uid : 0;
            var result = await _invoiceService.UndoMergeAsync(id, userId);
            if (!result)
            {
                return BadRequest(ApiResponse<object>.Fail("Không thể hoàn tác gộp hóa đơn. Hóa đơn không tồn tại hoặc không ở trạng thái đã gộp hợp lệ."));
            }
            return Ok(ApiResponse<object>.Ok(null, "Đã hoàn tác gộp hóa đơn"));
        }

        // [HttpPost("create-from-order/{orderId}")]
        // // [RequirePermission(PermissionCodes.InvoiceCreate)]
        // public async Task<IActionResult> CreateFromOrder(int orderId)
        // {
        //     var userId = GetCurrentUserId();
        //     var result = await _invoiceService.CreateInvoiceFromOrderAsync(orderId, userId);
        //     if (!result.Success)
        //         return BadRequest(result);
        //     return Ok(result);
        // }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
        {
            try
            {
                var result = await _invoiceService.UpdateStatusAsync(id, status, GetCurrentUserId());
                if (!result)
                    return NotFound(ApiResponse<object>.Fail("Hóa đơn không tồn tại"));
                return Ok(ApiResponse<object>.Ok(null, $"Đã cập nhật trạng thái hóa đơn thành: {InvoiceStatuses.ToDisplayText(status)}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportExcel([FromQuery] string? status)
        {
            var bytes = await _invoiceService.ExportInvoicesToExcelAsync(status);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"HoaDon_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        [HttpGet("customer/{customerId}/paged")]
        public async Task<IActionResult> GetByCustomerIdPaged(int customerId, [FromQuery] InvoiceFilterRequest request)
        {
            var paged = await _invoiceService.GetByCustomerIdPagedAsync(customerId, request);
            return Ok(ApiResponse<PagedResult<InvoiceDto>>.Ok(paged));
        }
    }
}