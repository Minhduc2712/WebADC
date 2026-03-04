using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebAPI.Attributes;
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
            return Ok(list);
        }

        [HttpGet("for-merge")]
        public async Task<IActionResult> GetForMerge()
        {
            var list = await _invoiceService.GetForMergeAsync(TryGetCurrentUserId());
            return Ok(list);
        }

        [HttpGet("for-warehouse-export")]
        public async Task<IActionResult> GetForWarehouseExport()
        {
            var list = await _invoiceService.GetForWarehouseExportSelectAsync(TryGetCurrentUserId());
            return Ok(list);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetAllPaged([FromQuery] InvoiceFilterRequest request)
        {
            var result = await _invoiceService.GetAllPagedAsync(request, TryGetCurrentUserId());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id, TryGetCurrentUserId());
            if (invoice == null) return NotFound();
            return Ok(invoice);
        }

        [HttpPost("split")]
        public async Task<IActionResult> SplitInvoice([FromBody] SplitInvoiceDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _invoiceService.SplitInvoiceAsync(dto, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("merge")]
        public async Task<IActionResult> MergeInvoices([FromBody] MergeInvoicesDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _invoiceService.MergeInvoicesAsync(dto, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("{id}/undo-split")]
        public async Task<IActionResult> UndoSplit(int id)
        {
            var userId = int.TryParse(User.FindFirst("UserId")?.Value, out int uid) ? uid : 0;
            var result = await _invoiceService.UndoSplitAsync(id, userId);
            if (!result)
            {
                return BadRequest(new { message = "Không thể hoàn tác" });
            }
            return Ok(new { success = true, message = "Đã hoàn tác tách hóa đơn" });
        }
        [HttpPost("{id}/undo-merge")]
        public async Task<IActionResult> UndoMerge(int id)
        {
            var userId = int.TryParse(User.FindFirst("UserId")?.Value, out int uid) ? uid : 0;
            var result = await _invoiceService.UndoMergeAsync(id, userId);
            if (!result)
            {
                return BadRequest(new { message = "Không thể hoàn tác" });
            }
            return Ok(new { success = true, message = "Đã hoàn tác gộp hóa đơn" });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportExcel([FromQuery] string? status)
        {
            var bytes = await _invoiceService.ExportInvoicesToExcelAsync(status);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"HoaDon_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }
}