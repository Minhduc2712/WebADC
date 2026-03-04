using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.WarehouseDTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.WarehouseView)]
    public class WarehouseController : BaseController
    {
        private readonly IWarehouseApiClient _warehouseApiClient;
        private readonly IProvinceApiClient _provinceApiClient;
        private readonly ILogger<WarehouseController> _logger;

        public WarehouseController(
            IWarehouseApiClient warehouseApiClient,
            IProvinceApiClient provinceApiClient,
            ILogger<WarehouseController> logger)
        {
            _warehouseApiClient = warehouseApiClient;
            _provinceApiClient = provinceApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? provinceId)
        {
            try
            {
                IEnumerable<WarehouseDto> warehouses;

                if (provinceId.HasValue && provinceId.Value > 0)
                    warehouses = await _warehouseApiClient.GetByProvinceIdAsync(provinceId.Value);
                else
                    warehouses = await _warehouseApiClient.GetAllAsync();

                var provinces = await _provinceApiClient.GetAllAsync();
                ViewBag.Provinces = new SelectList(provinces, "Id", "Province_name", provinceId);
                ViewBag.SelectedProvinceId = provinceId;

                return View(warehouses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading warehouses");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return View(Enumerable.Empty<WarehouseDto>());
            }
        }

        [RequirePermission(PermissionCodes.WarehouseCreate)]
        public async Task<IActionResult> Create()
        {
            try
            {
                var provinces = await _provinceApiClient.GetAllAsync();
                ViewBag.Provinces = new SelectList(provinces, "Id", "Province_name");
                return View(new CreateWarehouseDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create form");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.WarehouseCreate)]
        public async Task<IActionResult> Create(CreateWarehouseDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var provinces = await _provinceApiClient.GetAllAsync();
                    ViewBag.Provinces = new SelectList(provinces, "Id", "Province_name", model.Province_id);
                    return View(model);
                }

                var created = await _warehouseApiClient.CreateAsync(model);
                if (created != null)
                {
                    TempData["SuccessMessage"] = "Thêm kho hàng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Thêm kho hàng thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating warehouse");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            var provinceList = await _provinceApiClient.GetAllAsync();
            ViewBag.Provinces = new SelectList(provinceList, "Id", "Province_name", model.Province_id);
            return View(model);
        }

        [RequirePermission(PermissionCodes.WarehouseUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var warehouse = await _warehouseApiClient.GetByIdAsync(id);
                if (warehouse == null)
                    return NotFound();

                var provinces = await _provinceApiClient.GetAllAsync();
                ViewBag.Provinces = new SelectList(provinces, "Id", "Province_name", warehouse.Province_id);

                var updateDto = new UpdateWarehouseDto
                {
                    Id = warehouse.Id,
                    Warehouse_code = warehouse.Warehouse_code,
                    Warehouse_name = warehouse.Warehouse_name,
                    Warehouse_address = warehouse.Warehouse_address,
                    Province_id = warehouse.Province_id
                };
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading warehouse for edit");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.WarehouseUpdate)]
        public async Task<IActionResult> Edit(int id, UpdateWarehouseDto model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                {
                    var provinces = await _provinceApiClient.GetAllAsync();
                    ViewBag.Provinces = new SelectList(provinces, "Id", "Province_name", model.Province_id);
                    return View(model);
                }

                var (success, error) = await _warehouseApiClient.UpdateAsync(id, model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật kho hàng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Cập nhật thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            var provinceList = await _provinceApiClient.GetAllAsync();
            ViewBag.Provinces = new SelectList(provinceList, "Id", "Province_name", model.Province_id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.WarehouseDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, error) = await _warehouseApiClient.DeleteAsync(id);
                if (success)
                    TempData["SuccessMessage"] = "Xóa kho hàng thành công!";
                else
                    TempData["ErrorMessage"] = error ?? "Xóa thất bại.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting warehouse");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
