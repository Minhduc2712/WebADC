using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Attributes;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.WarehouseView)]
    public class WarehouseController : BaseController
    {
        private readonly IWarehouseService _warehouseService;
        private readonly IProvinceService _provinceService;
        private readonly ILogger<WarehouseController> _logger;

        public WarehouseController(
            IWarehouseService warehouseService,
            IProvinceService provinceService,
            ILogger<WarehouseController> logger)
        {
            _warehouseService = warehouseService;
            _provinceService = provinceService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? provinceId)
        {
            try
            {
                IEnumerable<Warehouse> warehouses;

                if (provinceId.HasValue && provinceId.Value > 0)
                {
                    warehouses = await _warehouseService.GetByProvinceIdAsync(provinceId.Value);
                }
                else
                {
                    warehouses = await _warehouseService.GetAllAsync();
                }

                var provinces = await _provinceService.GetAllAsync();
                ViewBag.Provinces = new SelectList(provinces, "Id", "Province_name", provinceId);
                ViewBag.SelectedProvinceId = provinceId;

                return View(warehouses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading warehouses");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return View(Enumerable.Empty<Warehouse>());
            }
        }

        [RequirePermission(PermissionCodes.WarehouseCreate)]
        public async Task<IActionResult> Create()
        {
            try
            {
                var provinces = await _provinceService.GetAllAsync();
                ViewBag.Provinces = new SelectList(provinces, "Id", "Province_name");
                return View();
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
        public async Task<IActionResult> Create(Warehouse model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var provinces = await _provinceService.GetAllAsync();
                    ViewBag.Provinces = new SelectList(provinces, "Id", "Province_name", model.Province_id);
                    return View(model);
                }

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                model.Created_by = userId;
                model.Updated_by = userId;

                await _warehouseService.CreateAsync(model);
                TempData["SuccessMessage"] = "Thêm kho hàng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating warehouse");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                var provinces = await _provinceService.GetAllAsync();
                ViewBag.Provinces = new SelectList(provinces, "Id", "Province_name", model.Province_id);
                return View(model);
            }
        }

        [RequirePermission(PermissionCodes.WarehouseUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var warehouse = await _warehouseService.GetByIdAsync(id);
                if (warehouse == null)
                    return NotFound();

                var provinces = await _provinceService.GetAllAsync();
                ViewBag.Provinces = new SelectList(provinces, "Id", "Province_name", warehouse.Province_id);

                return View(warehouse);
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
        public async Task<IActionResult> Edit(int id, Warehouse model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                {
                    var provinces = await _provinceService.GetAllAsync();
                    ViewBag.Provinces = new SelectList(provinces, "Id", "Province_name", model.Province_id);
                    return View(model);
                }

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                model.Updated_by = userId;

                await _warehouseService.UpdateAsync(model);
                TempData["SuccessMessage"] = "Cập nhật kho hàng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                var provinces = await _provinceService.GetAllAsync();
                ViewBag.Provinces = new SelectList(provinces, "Id", "Province_name", model.Province_id);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.WarehouseDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _warehouseService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xóa kho hàng thành công!";
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
