using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.WebMVC.Attributes;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.WarehouseView)]
    public class WarehouseExportController : BaseController
    {
        private readonly IWarehouseExportService _warehouseExportService;
        private readonly IInvoiceService _invoiceService;
        private readonly IWarehouseService _warehouseService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<WarehouseExportController> _logger;

        public WarehouseExportController(
            IWarehouseExportService warehouseExportService,
            IInvoiceService invoiceService,
            IWarehouseService warehouseService,
            IPermissionService permissionService,
            ILogger<WarehouseExportController> logger)
        {
            _warehouseExportService = warehouseExportService;
            _invoiceService = invoiceService;
            _warehouseService = warehouseService;
            _permissionService = permissionService;
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
                var exports = await _warehouseExportService.GetAllAsync();

                if (!string.IsNullOrEmpty(status))
                {
                    exports = exports.Where(e => e.Status == status);
                }

                ViewBag.Status = status;
                await LoadCurrentUserPermissions();
                return View(exports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading exports");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(Enumerable.Empty<WarehouseExportDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var export = await _warehouseExportService.GetByIdAsync(id);
                if (export == null)
                    return NotFound();

                await LoadCurrentUserPermissions();
                return View(export);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading export details");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission(PermissionCodes.WarehouseCreate)]
        public async Task<IActionResult> Create()
        {
            try
            {
                var invoices = await _invoiceService.GetAllAsync();
                var warehouses = await _warehouseService.GetAllAsync();

                ViewBag.Invoices = new SelectList(
                    invoices.Where(i => i.Status != "Cancelled"),
                    "Id",
                    "Invoice_code");
                ViewBag.Warehouses = new SelectList(warehouses, "Id", "Warehouse_name");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create form");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.WarehouseCreate)]
        public async Task<IActionResult> Create(CreateWarehouseExportDto model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _warehouseExportService.CreateExportFromInvoiceAsync(model, userId);

                if (result != null)
                {
                    SetSuccessMessage("Tạo phiếu xuất kho thành công!");
                    return RedirectToAction(nameof(Details), new { id = result.Id });
                }
                else
                {
                    SetErrorMessage("Không thể tạo phiếu xuất kho!");
                    var invoices = await _invoiceService.GetAllAsync();
                    var warehouses = await _warehouseService.GetAllAsync();
                    ViewBag.Invoices = new SelectList(invoices, "Id", "Invoice_code");
                    ViewBag.Warehouses = new SelectList(warehouses, "Id", "Warehouse_name");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating export");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.WarehouseUpdate)]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _warehouseExportService.ConfirmExportAsync(id, userId);

                if (result)
                    SetSuccessMessage("Đã xác nhận xuất kho thành công!");
                else
                    SetErrorMessage("Không thể xác nhận xuất kho!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming export");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.WarehouseUpdate)]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _warehouseExportService.CancelExportAsync(id, userId);

                if (result)
                    SetSuccessMessage("Đã hủy xuất kho thành công!");
                else
                    SetErrorMessage("Không thể hủy xuất kho!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling export");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.WarehouseUpdate)]
        public async Task<IActionResult> UpdateDeliveryStatus(int id, string status)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _warehouseExportService.UpdateDeliveryStatusAsync(id, status, userId);

                if (result)
                    SetSuccessMessage("Đã cập nhật trạng thái giao hàng!");
                else
                    SetErrorMessage("Không thể cập nhật trạng thái!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery status");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.WarehouseDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _warehouseExportService.DeleteExportAsync(id);

                if (result)
                    SetSuccessMessage("Đã xóa phiếu xuất kho thành công!");
                else
                    SetErrorMessage("Không thể xóa phiếu xuất kho!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting export");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.WarehouseUpdate)]
        public async Task<IActionResult> Split(int id)
        {
            try
            {
                var export = await _warehouseExportService.GetByIdAsync(id);
                if (export == null)
                    return NotFound();

                return View(export);
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
        [RequirePermission(PermissionCodes.WarehouseUpdate)]
        public async Task<IActionResult> Split(SplitWarehouseExportDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _warehouseExportService.SplitExportAsync(dto, userId);

                if (result.Success)
                    SetSuccessMessage(result.Message ?? "Tách phiếu xuất kho thành công!");
                else
                    SetErrorMessage(result.Message ?? "Tách phiếu xuất kho thất bại!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error splitting export");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.WarehouseUpdate)]
        public async Task<IActionResult> Merge()
        {
            try
            {
                var exports = await _warehouseExportService.GetAllAsync();
                return View(exports.Where(e => e.Status != "Completed" && e.Status != "Cancelled"));
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
        [RequirePermission(PermissionCodes.WarehouseUpdate)]
        public async Task<IActionResult> Merge(MergeWarehouseExportsDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _warehouseExportService.MergeExportsAsync(dto, userId);

                if (result.Success)
                    SetSuccessMessage(result.Message ?? "Gộp phiếu xuất kho thành công!");
                else
                    SetErrorMessage(result.Message ?? "Gộp phiếu xuất kho thất bại!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging exports");
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
                var permissions = await _permissionService.GetUserPermissionsAsync(userId);
                ViewBag.CurrentUserPermissions = permissions?.ToList() ?? new List<string>();

                ViewBag.CanCreate = permissions?.Contains(PermissionCodes.WarehouseCreate) ?? false;
                ViewBag.CanUpdate = permissions?.Contains(PermissionCodes.WarehouseUpdate) ?? false;
                ViewBag.CanDelete = permissions?.Contains(PermissionCodes.WarehouseDelete) ?? false;
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
