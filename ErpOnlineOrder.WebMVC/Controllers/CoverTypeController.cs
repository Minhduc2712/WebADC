using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Attributes;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission("CATEGORY_VIEW")]
    public class CoverTypeController : BaseController
    {
        private readonly ICoverTypeService _coverTypeService;
        private readonly ILogger<CoverTypeController> _logger;

        public CoverTypeController(ICoverTypeService coverTypeService, ILogger<CoverTypeController> logger)
        {
            _coverTypeService = coverTypeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var coverTypes = await _coverTypeService.GetAllAsync();
                return View(coverTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cover types");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return View(Enumerable.Empty<Cover_type>());
            }
        }

        [RequirePermission("CATEGORY_CREATE")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CATEGORY_CREATE")]
        public async Task<IActionResult> Create(Cover_type model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                model.Created_by = userId;
                model.Updated_by = userId;

                await _coverTypeService.CreateAsync(model);
                TempData["SuccessMessage"] = "Thêm loại bìa thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cover type");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                return View(model);
            }
        }

        [RequirePermission("CATEGORY_UPDATE")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var coverType = await _coverTypeService.GetByIdAsync(id);
                if (coverType == null)
                    return NotFound();

                return View(coverType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cover type for edit");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CATEGORY_UPDATE")]
        public async Task<IActionResult> Edit(int id, Cover_type model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(model);

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                model.Updated_by = userId;

                await _coverTypeService.UpdateAsync(model);
                TempData["SuccessMessage"] = "Cập nhật loại bìa thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cover type");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CATEGORY_DELETE")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _coverTypeService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xóa loại bìa thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cover type");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
