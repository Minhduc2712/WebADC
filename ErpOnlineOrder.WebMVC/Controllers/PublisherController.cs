using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Attributes;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission("CATEGORY_VIEW")]
    public class PublisherController : BaseController
    {
        private readonly IPublisherService _publisherService;
        private readonly ILogger<PublisherController> _logger;

        public PublisherController(IPublisherService publisherService, ILogger<PublisherController> logger)
        {
            _publisherService = publisherService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var publishers = await _publisherService.GetAllAsync();
                return View(publishers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading publishers");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return View(Enumerable.Empty<Publisher>());
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
        public async Task<IActionResult> Create(Publisher model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                model.Created_by = userId;
                model.Updated_by = userId;

                await _publisherService.CreateAsync(model);
                TempData["SuccessMessage"] = "Thêm nhà xuất bản thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating publisher");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                return View(model);
            }
        }

        [RequirePermission("CATEGORY_UPDATE")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var publisher = await _publisherService.GetByIdAsync(id);
                if (publisher == null)
                    return NotFound();

                return View(publisher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading publisher for edit");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CATEGORY_UPDATE")]
        public async Task<IActionResult> Edit(int id, Publisher model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(model);

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                model.Updated_by = userId;

                await _publisherService.UpdateAsync(model);
                TempData["SuccessMessage"] = "Cập nhật nhà xuất bản thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating publisher");
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
                await _publisherService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xóa nhà xuất bản thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting publisher");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
