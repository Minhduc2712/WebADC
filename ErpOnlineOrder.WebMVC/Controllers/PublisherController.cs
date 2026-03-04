using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.PublisherDTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Services;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission("CATEGORY_VIEW")]
    public class PublisherController : BaseController
    {
        private readonly IPublisherApiClient _publisherApiClient;
        private readonly ILogger<PublisherController> _logger;

        public PublisherController(IPublisherApiClient publisherApiClient, ILogger<PublisherController> logger)
        {
            _publisherApiClient = publisherApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var publishers = await _publisherApiClient.GetAllAsync();
                return View(publishers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading publishers");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return View(Enumerable.Empty<PublisherDto>());
            }
        }

        [RequirePermission("CATEGORY_CREATE")]
        public IActionResult Create()
        {
            return View(new CreatePublisherDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CATEGORY_CREATE")]
        public async Task<IActionResult> Create(CreatePublisherDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var created = await _publisherApiClient.CreateAsync(model);
                if (created != null)
                {
                    TempData["SuccessMessage"] = "Thêm nhà xuất bản thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Thêm nhà xuất bản thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating publisher");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            return View(model);
        }

        [RequirePermission("CATEGORY_UPDATE")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var publisher = await _publisherApiClient.GetByIdAsync(id);
                if (publisher == null)
                    return NotFound();

                var updateDto = new UpdatePublisherDto
                {
                    Id = publisher.Id,
                    Publisher_code = publisher.Publisher_code,
                    Publisher_name = publisher.Publisher_name,
                    Publisher_address = publisher.Publisher_address,
                    Publisher_phone = publisher.Publisher_phone,
                    Publisher_email = publisher.Publisher_email
                };
                return View(updateDto);
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
        public async Task<IActionResult> Edit(int id, UpdatePublisherDto model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(model);

                var (success, error) = await _publisherApiClient.UpdateAsync(id, model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật nhà xuất bản thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Cập nhật thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating publisher");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CATEGORY_DELETE")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, error) = await _publisherApiClient.DeleteAsync(id);
                if (success)
                    TempData["SuccessMessage"] = "Xóa nhà xuất bản thành công!";
                else
                    TempData["ErrorMessage"] = error ?? "Xóa thất bại.";
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
