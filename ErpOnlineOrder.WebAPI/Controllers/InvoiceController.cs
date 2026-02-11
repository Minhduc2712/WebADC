using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
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
            var list = await _invoiceService.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id);
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
    }
}