using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Domain.Constants;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    // [RequirePermission(PermissionCodes.WarehouseExportView)]
    public class WarehouseExportController : BaseController
    {
        private readonly IWarehouseExportApiClient _warehouseExportApiClient;
        private readonly IInvoiceApiClient _invoiceApiClient;
        private readonly IWarehouseApiClient _warehouseApiClient;
        private readonly IPermissionApiClient _permissionApiClient;
        private readonly ILogger<WarehouseExportController> _logger;

        public WarehouseExportController(
            IWarehouseExportApiClient warehouseExportApiClient,
            IInvoiceApiClient invoiceApiClient,
            IWarehouseApiClient warehouseApiClient,
            IPermissionApiClient permissionApiClient,
            ILogger<WarehouseExportController> logger)
        {
            _warehouseExportApiClient = warehouseExportApiClient;
            _invoiceApiClient = invoiceApiClient;
            _warehouseApiClient = warehouseApiClient;
            _permissionApiClient = permissionApiClient;
            _logger = logger;
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
                {
                    SetErrorMessage("Không tìm thấy phiếu xuất kho.");
                    return RedirectToAction(nameof(Index));
                }

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

        [HttpGet]
        // [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var export = await _warehouseExportApiClient.GetByIdAsync(id);
                if (export == null)
                {
                    SetErrorMessage("Không tìm thấy phiếu xuất kho.");
                    return RedirectToAction(nameof(Index));
                }

                if (export.Status != ExportStatuses.Draft)
                {
                    SetErrorMessage("Chỉ có thể chỉnh sửa phiếu xuất kho ở trạng thái Nháp.");
                    return RedirectToAction(nameof(Details), new { id });
                }

                var warehouses = await _warehouseApiClient.GetForSelectAsync();
                ViewBag.Warehouses = new SelectList(warehouses, "Id", "Warehouse_name", export.Warehouse_id);
                ViewBag.ExportCode = export.Warehouse_export_code;
                ViewBag.ExportId = export.Id;

                var model = new UpdateWarehouseExportDto
                {
                    Warehouse_id = export.Warehouse_id,
                    Export_date = export.Export_date,
                    Arrival_date = export.Arrival_date,
                    Split_merge_note = export.Split_merge_note
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading export edit form");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> Edit(int id, UpdateWarehouseExportDto model)
        {
            try
            {
                var (success, error) = await _warehouseExportApiClient.UpdateAsync(id, model);
                if (success)
                {
                    SetSuccessMessage("Cập nhật phiếu xuất kho thành công!");
                    return RedirectToAction(nameof(Details), new { id });
                }

                SetErrorMessage(error ?? "Không thể cập nhật phiếu xuất kho. Chỉ phiếu ở trạng thái Nháp mới được chỉnh sửa.");
                var export = await _warehouseExportApiClient.GetByIdAsync(id);
                var warehouses = await _warehouseApiClient.GetForSelectAsync();
                ViewBag.Warehouses = new SelectList(warehouses, "Id", "Warehouse_name", model.Warehouse_id);
                ViewBag.ExportCode = export?.Warehouse_export_code ?? "";
                ViewBag.ExportId = id;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating export");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                var (result, error) = await _warehouseExportApiClient.ConfirmAsync(id);

                if (result)
                    SetSuccessMessage("Đã xác nhận xuất kho thành công!");
                else
                    SetErrorMessage(error ?? "Không thể xác nhận xuất kho. Vui lòng kiểm tra trạng thái phiếu hiện tại.");
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
        // [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var (result, error) = await _warehouseExportApiClient.CancelAsync(id);

                if (result)
                    SetSuccessMessage("Đã hủy xuất kho thành công!");
                else
                    SetErrorMessage(error ?? "Không thể hủy xuất kho. Phiếu đã giao hàng hoặc đã hoàn thành không thể hủy.");
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
        // [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> UpdateDeliveryStatus(int id, string status)
        {
            try
            {
                var (result, error) = await _warehouseExportApiClient.UpdateDeliveryStatusAsync(id, status);

                if (result)
                    SetSuccessMessage("Đã cập nhật trạng thái giao hàng!");
                else
                    SetErrorMessage(error ?? "Không thể cập nhật trạng thái giao hàng. Vui lòng kiểm tra luồng trạng thái hợp lệ.");
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
        // [RequirePermission(PermissionCodes.WarehouseExportDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (result, error) = await _warehouseExportApiClient.DeleteAsync(id);

                if (result)
                    SetSuccessMessage("Đã xóa phiếu xuất kho thành công!");
                else
                    SetErrorMessage(error ?? "Không thể xóa phiếu xuất kho. Chỉ được xóa khi phiếu ở trạng thái Nháp và chưa giao hàng.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting export");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        // [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> Split(int id)
        {
            try
            {
                var export = await _warehouseExportApiClient.GetByIdAsync(id);
                if (export == null)
                {
                    SetErrorMessage("Không tìm thấy phiếu xuất kho.");
                    return RedirectToAction(nameof(Index));
                }

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
        // [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
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
        // [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> Merge()
        {
            try
            {
                var exports = await _warehouseExportApiClient.GetMergeableAsync();
                return View(exports);
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
        // [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
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
        // [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> UndoSplit(int id)
        {
            try
            {
                var result = await _warehouseExportApiClient.UndoSplitAsync(id);
                if (result)
                    SetSuccessMessage("Đã hoàn tác tách phiếu xuất kho!");
                else
                    SetErrorMessage("Không thể hoàn tác tách phiếu xuất kho. Vui lòng kiểm tra phiếu cha có ở trạng thái Đã tách và các phiếu con chưa phát sinh xử lý.");
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
        // [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> UndoMerge(int id)
        {
            try
            {
                var result = await _warehouseExportApiClient.UndoMergeAsync(id);
                if (result)
                    SetSuccessMessage("Đã hoàn tác gộp phiếu xuất kho!");
                else
                    SetErrorMessage("Không thể hoàn tác gộp phiếu xuất kho. Vui lòng kiểm tra trạng thái phiếu và dữ liệu phiếu nguồn.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error undoing merge");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }
            return RedirectToAction(nameof(Index));
        }

        // [RequirePermission(PermissionCodes.WarehouseExportView)]
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string? status)
        {
            try
            {
                var bytes = await _warehouseExportApiClient.ExportToExcelAsync(status);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        // [RequirePermission(PermissionCodes.WarehouseExportUpdate)]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var (success, error) = await _warehouseExportApiClient.UpdateStatusAsync(id, status);
                if (success)
                    SetSuccessMessage($"Đã cập nhật trạng thái phiếu xuất kho thành: {ExportStatuses.ToDisplayText(status)}");
                else
                    SetErrorMessage(error ?? "Không thể cập nhật trạng thái phiếu xuất kho. Vui lòng kiểm tra trạng thái hiện tại và trạng thái đích.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating export status");
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
