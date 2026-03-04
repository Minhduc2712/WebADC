using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.WarehouseExportView)]
    public class WarehouseExportController : BaseController
    {
        private readonly IWarehouseExportApiClient _warehouseExportApiClient;
        private readonly IWarehouseExportService _warehouseExportService;
        private readonly IInvoiceApiClient _invoiceApiClient;
        private readonly IWarehouseApiClient _warehouseApiClient;
        private readonly IPermissionApiClient _permissionApiClient;
        private readonly ILogger<WarehouseExportController> _logger;

        public WarehouseExportController(
            IWarehouseExportApiClient warehouseExportApiClient,
            IWarehouseExportService warehouseExportService,
            IInvoiceApiClient invoiceApiClient,
            IWarehouseApiClient warehouseApiClient,
            IPermissionApiClient permissionApiClient,
            ILogger<WarehouseExportController> logger)
        {
            _warehouseExportApiClient = warehouseExportApiClient;
            _warehouseExportService = warehouseExportService;
            _invoiceApiClient = invoiceApiClient;
            _warehouseApiClient = warehouseApiClient;
            _permissionApiClient = permissionApiClient;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? status = null, string? search = null)
        {
            try
            {
                var result = await _warehouseExportApiClient.GetPagedAsync(page, pageSize, status, search);
                ViewBag.Status = status;
                ViewBag.PageSize = pageSize;
                ViewBag.Search = search;
                await LoadCurrentUserPermissions();
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading exports");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(new PagedResult<WarehouseExportDto> { Items = new List<WarehouseExportDto>() });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var export = await _warehouseExportApiClient.GetByIdAsync(id);
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

        [RequirePermission(PermissionCodes.WarehouseExportCreate)]
        public async Task<IActionResult> Create()
        {
            try
            {
                var invoices = await _invoiceApiClient.GetForWarehouseExportAsync();
                var warehouses = await _warehouseApiClient.GetForSelectAsync();

                ViewBag.Invoices = new SelectList(invoices, "Id", "Invoice_code");
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
        [RequirePermission(PermissionCodes.WarehouseExportCreate)]
        public async Task<IActionResult> Create(CreateWarehouseExportDto model)
        {
            try
            {
                var result = await _warehouseExportApiClient.CreateAsync(model);

                if (result != null)
                {
                    SetSuccessMessage("Tạo phiếu xuất kho thành công!");
                    return RedirectToAction(nameof(Details), new { id = result.Id });
                }
                else
                {
                    SetErrorMessage("Không thể tạo phiếu xuất kho!");
                    var invoices = await _invoiceApiClient.GetForWarehouseExportAsync();
                    var warehouses = await _warehouseApiClient.GetForSelectAsync();
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
        [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                var (result, _) = await _warehouseExportApiClient.ConfirmAsync(id);

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
        [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var (result, _) = await _warehouseExportApiClient.CancelAsync(id);

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
        [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> UpdateDeliveryStatus(int id, string status)
        {
            try
            {
                var (result, _) = await _warehouseExportApiClient.UpdateDeliveryStatusAsync(id, status);

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
        [RequirePermission(PermissionCodes.WarehouseExportDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (result, _) = await _warehouseExportApiClient.DeleteAsync(id);

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
        [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> Split(int id)
        {
            try
            {
                var export = await _warehouseExportApiClient.GetByIdAsync(id);
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
        [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> Split(SplitWarehouseExportDto dto)
        {
            try
            {
                var result = await _warehouseExportApiClient.SplitAsync(dto);

                if (result?.Success == true)
                    SetSuccessMessage(result.Message ?? "Tách phiếu xuất kho thành công!");
                else
                    SetErrorMessage(result?.Message ?? "Tách phiếu xuất kho thất bại!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error splitting export");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> Merge()
        {
            try
            {
                var exports = await _warehouseExportApiClient.GetAllAsync();
                return View(exports.Where(e => e.Status != "Completed" && e.Status != "Cancelled" && e.Status != "Merged"));
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
        [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> Merge(MergeWarehouseExportsDto dto)
        {
            try
            {
                var result = await _warehouseExportApiClient.MergeAsync(dto);

                if (result?.Success == true)
                    SetSuccessMessage(result.Message ?? "Gộp phiếu xuất kho thành công!");
                else
                    SetErrorMessage(result?.Message ?? "Gộp phiếu xuất kho thất bại!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging exports");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> UndoSplit(int id)
        {
            try
            {
                var result = await _warehouseExportApiClient.UndoSplitAsync(id);
                if (result)
                    SetSuccessMessage("Đã hoàn tác tách phiếu xuất kho!");
                else
                    SetErrorMessage("Không thể hoàn tác tách phiếu xuất kho!");
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
        [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> UndoMerge(int id)
        {
            try
            {
                var result = await _warehouseExportApiClient.UndoMergeAsync(id);
                if (result)
                    SetSuccessMessage("Đã hoàn tác gộp phiếu xuất kho!");
                else
                    SetErrorMessage("Không thể hoàn tác gộp phiếu xuất kho!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error undoing merge");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }
            return RedirectToAction(nameof(Index));
        }

        [RequirePermission(PermissionCodes.WarehouseExportView)]
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string? status)
        {
            try
            {
                var bytes = await _warehouseExportService.ExportWarehouseExportsToExcelAsync(status);
                if (bytes == null || bytes.Length == 0)
                {
                    SetErrorMessage("Không có dữ liệu để xuất.");
                    return RedirectToAction(nameof(Index));
                }
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"PhieuXuatKho_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting warehouse exports");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
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

                ViewBag.CanCreate = permissions?.Contains(PermissionCodes.WarehouseExportCreate) ?? false;
                ViewBag.CanUpdate = permissions?.Contains(PermissionCodes.WarehouseExportUpdate) ?? false;
                ViewBag.CanDelete = permissions?.Contains(PermissionCodes.WarehouseExportDelete) ?? false;
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
