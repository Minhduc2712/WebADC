using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Attributes;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.DistributorView)]
    public class DistributorController : BaseController
    {
        private readonly IDistributorService _distributorService;
        private readonly ILogger<DistributorController> _logger;

        public DistributorController(IDistributorService distributorService, ILogger<DistributorController> logger)
        {
            _distributorService = distributorService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var distributors = await _distributorService.GetAllAsync();
                return View(distributors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading distributors");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return View(Enumerable.Empty<Distributor>());
            }
        }

        [RequirePermission(PermissionCodes.DistributorCreate)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.DistributorCreate)]
        public async Task<IActionResult> Create(Distributor model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                model.Created_by = userId;
                model.Updated_by = userId;
                model.Created_at = DateTime.Now;
                model.Updated_at = DateTime.Now;

                await _distributorService.CreateDistributorAsync(model);
                TempData["SuccessMessage"] = "Thêm nhà phân phối thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating distributor");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                return View(model);
            }
        }

        [RequirePermission(PermissionCodes.DistributorUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var distributor = await _distributorService.GetByIdAsync(id);
                if (distributor == null)
                    return NotFound();

                return View(distributor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading distributor for edit");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.DistributorUpdate)]
        public async Task<IActionResult> Edit(int id, Distributor model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(model);

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                model.Updated_by = userId;
                model.Updated_at = DateTime.Now;

                await _distributorService.UpdateDistributorAsync(model);
                TempData["SuccessMessage"] = "Cập nhật nhà phân phối thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating distributor");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.DistributorDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _distributorService.DeleteDistributorAsync(id);
                TempData["SuccessMessage"] = "Xóa nhà phân phối thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting distributor");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
