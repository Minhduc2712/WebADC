using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;

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

        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        public async Task<IActionResult> Index(string? status)
        {
            try
            {
                var invoices = await _invoiceApiClient.GetAllAsync();

                if (!string.IsNullOrEmpty(status))
                {
                    invoices = invoices.Where(i => i.Status == status);
                }

                ViewBag.Status = status;
                await LoadCurrentUserPermissions();
                return View(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading invoices");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(Enumerable.Empty<InvoiceDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var invoice = await _invoiceApiClient.GetByIdAsync(id);

                if (invoice == null)
                    return NotFound();

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
                    return NotFound();

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
                    SetErrorMessage(result.Message ?? "Tách hóa đơn thất bại!");
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
                var invoices = await _invoiceApiClient.GetAllAsync();
                return View(invoices.Where(i => i.Status != "Completed" && i.Status != "Cancelled"));
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
                    SetErrorMessage(result.Message ?? "Gộp hóa đơn thất bại!");
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
                    SetErrorMessage("Không thể hoàn tác tách hóa đơn!");
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
                    SetErrorMessage("Không thể hoàn tác gộp hóa đơn!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error undoing merge");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
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
