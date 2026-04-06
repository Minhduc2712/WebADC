using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Domain.Constants;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.InvoiceView)]
    public class InvoiceController : BaseController
    {
        private readonly IInvoiceApiClient _invoiceApiClient;
        private readonly IPermissionApiClient _permissionApiClient;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
            IInvoiceApiClient invoiceApiClient,
            IPermissionApiClient permissionApiClient,
            ILogger<InvoiceController> logger)
        {
            _invoiceApiClient = invoiceApiClient;
            _permissionApiClient = permissionApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? status = null, string? search = null)
        {
            try
            {
                var result = await _invoiceApiClient.GetPagedAsync(page, pageSize, status, search);
                ViewBag.Status = status;
                ViewBag.PageSize = pageSize;
                ViewBag.Search = search;
                await LoadCurrentUserPermissions();
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading invoices");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(new PagedResult<InvoiceDto> { Items = new List<InvoiceDto>() });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var invoice = await _invoiceApiClient.GetByIdAsync(id);

                if (invoice == null)
                {
                    SetErrorMessage("Không tìm thấy hóa đơn.");
                    return RedirectToAction(nameof(Index));
                }

                await LoadCurrentUserPermissions();
                return View(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading invoice details");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.InvoiceUpdate)]
        public async Task<IActionResult> Split(int id)
        {
            try
            {
                var invoice = await _invoiceApiClient.GetByIdAsync(id);
                if (invoice == null)
                {
                    SetErrorMessage("Không tìm thấy hóa đơn.");
                    return RedirectToAction(nameof(Index));
                }

                return View(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading split form");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.InvoiceUpdate)]
        public async Task<IActionResult> Split(SplitInvoiceDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _invoiceApiClient.SplitAsync(dto);

                if (result?.Success == true)
                {
                    SetSuccessMessage($"Tách hóa đơn thành công! {result.Message}");
                }
                else
                {
                    SetErrorMessage(result.Message ?? "Không thể tách hóa đơn. Vui lòng kiểm tra trạng thái hóa đơn và số lượng sản phẩm cần tách.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error splitting invoice");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.InvoiceUpdate)]
        public async Task<IActionResult> Merge()
        {
            try
            {
                var invoices = await _invoiceApiClient.GetForMergeAsync();
                return View(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading merge form");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.InvoiceUpdate)]
        public async Task<IActionResult> Merge(MergeInvoicesDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _invoiceApiClient.MergeAsync(dto);

                if (result?.Success == true)
                {
                    SetSuccessMessage($"Gộp hóa đơn thành công! {result.Message}");
                }
                else
                {
                    SetErrorMessage(result.Message ?? "Không thể gộp hóa đơn. Vui lòng kiểm tra danh sách hóa đơn nguồn và điều kiện gộp.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging invoices");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.InvoiceUpdate)]
        public async Task<IActionResult> UndoSplit(int parentInvoiceId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _invoiceApiClient.UndoSplitAsync(parentInvoiceId);

                if (result)
                {
                    SetSuccessMessage("Hoàn tác tách hóa đơn thành công!");
                }
                else
                {
                    SetErrorMessage("Không thể hoàn tác tách hóa đơn. Hóa đơn cha có thể không ở trạng thái Đã tách hoặc hóa đơn con đã phát sinh xử lý.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error undoing split");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.InvoiceUpdate)]
        public async Task<IActionResult> UndoMerge(int mergedInvoiceId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _invoiceApiClient.UndoMergeAsync(mergedInvoiceId);

                if (result)
                {
                    SetSuccessMessage("Hoàn tác gộp hóa đơn thành công!");
                }
                else
                {
                    SetErrorMessage("Không thể hoàn tác gộp hóa đơn. Hóa đơn đã gộp có thể không còn đủ điều kiện hoàn tác.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error undoing merge");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        [RequirePermission(PermissionCodes.InvoiceView)]
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string? status)
        {
            try
            {
                var bytes = await _invoiceApiClient.ExportToExcelAsync(status);
                if (bytes == null || bytes.Length == 0)
                {
                    SetErrorMessage("Không có dữ liệu để xuất.");
                    return RedirectToAction(nameof(Index));
                }
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"HoaDon_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting invoices");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.InvoiceUpdate)]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var (success, error) = await _invoiceApiClient.UpdateStatusAsync(id, status);
                if (success)
                    SetSuccessMessage($"Đã cập nhật trạng thái hóa đơn thành: {InvoiceStatuses.ToDisplayText(status)}");
                else
                    SetErrorMessage(error ?? "Không thể cập nhật trạng thái hóa đơn. Vui lòng kiểm tra trạng thái hiện tại và trạng thái đích.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice status");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.InvoiceView)]
        public async Task<IActionResult> PrintInvoice(int id)
        {
            try
            {
                var invoice = await _invoiceApiClient.GetByIdAsync(id);
                if (invoice == null)
                {
                    SetErrorMessage("Không tìm thấy hóa đơn.");
                    return RedirectToAction(nameof(Index));
                }

                return View(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading print invoice {InvoiceId}", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.InvoiceView)]
        public async Task<IActionResult> DownloadDocument(int id, string format = "pdf", string template = "standard")
        {
            try
            {
                var bytes = await _invoiceApiClient.DownloadDocumentAsync(id, format, template);
                var contentType = format.ToLower() switch
                {
                    "pdf" => "application/pdf",
                    "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    _ => "application/xml"
                };
                var ext = format.ToLower() == "docx" ? "docx" : format.ToLower() == "pdf" ? "pdf" : "xml";
                return File(bytes, contentType, $"hoadon-{id}.{ext}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading invoice {InvoiceId} format {Format}", id, format);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(PrintInvoice), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.InvoiceView)]
        public async Task<IActionResult> SendToCustomer(int id)
        {
            try
            {
                var (success, error) = await _invoiceApiClient.SendToCustomerAsync(id);
                if (success)
                    SetSuccessMessage("Đã gửi hóa đơn cho khách hàng qua email.");
                else
                    SetErrorMessage(error ?? "Không thể gửi hóa đơn cho khách hàng.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invoice to customer");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task LoadCurrentUserPermissions()
        {
            try
            {
                var roles = HttpContext.Session.GetString("Roles") ?? "";
                if (roles.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains("ROLE_ADMIN"))
                {
                    ViewBag.CurrentUserPermissions = new List<string> { "ALL" };
                    ViewBag.CanCreate = true;
                    ViewBag.CanUpdate = true;
                    ViewBag.CanDelete = true;
                    return;
                }

                var userId = GetCurrentUserId();
                var permissions = await _permissionApiClient.GetUserPermissionCodesAsync(userId);
                ViewBag.CurrentUserPermissions = permissions?.ToList() ?? new List<string>();

                ViewBag.CanCreate = permissions?.Contains(PermissionCodes.InvoiceCreate) ?? false;
                ViewBag.CanUpdate = permissions?.Contains(PermissionCodes.InvoiceUpdate) ?? false;
                ViewBag.CanDelete = permissions?.Contains(PermissionCodes.InvoiceDelete) ?? false;
            }
            catch
            {
                ViewBag.CurrentUserPermissions = new List<string>();
                ViewBag.CanCreate = false;
                ViewBag.CanUpdate = false;
                ViewBag.CanDelete = false;
            }
        }
    }
}
