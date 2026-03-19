using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.PublisherDTOs;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Services;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.CategoryView)]
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
                SetErrorFromException(ex);
                return View(Enumerable.Empty<PublisherDto>());
            }
        }

        [RequirePermission(PermissionCodes.CategoryCreate)]
        public IActionResult Create()
        {
            return View(new CreatePublisherDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CategoryCreate)]
        public async Task<IActionResult> Create(CreatePublisherDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var (created, error) = await _publisherApiClient.CreateAsync(model);
                if (created != null)
                {
                    SetSuccessMessage("Thêm nhà xuất bản thành công!");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Không thể thêm nhà xuất bản. Vui lòng kiểm tra mã nhà xuất bản và thông tin liên hệ.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating publisher");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            return View(model);
        }

        [RequirePermission(PermissionCodes.CategoryUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var publisher = await _publisherApiClient.GetByIdAsync(id);
                if (publisher == null)
                {
                    SetErrorMessage("Không tìm thấy nhà xuất bản.");
                    return RedirectToAction(nameof(Index));
                }

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
                SetErrorFromException(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CategoryUpdate)]
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
                    SetSuccessMessage("Cập nhật nhà xuất bản thành công!");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Không thể cập nhật nhà xuất bản. Dữ liệu có thể không hợp lệ hoặc bản ghi không còn tồn tại.");
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
        [RequirePermission(PermissionCodes.CategoryDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, error) = await _publisherApiClient.DeleteAsync(id);
                if (success)
                    SetSuccessMessage("Xóa nhà xuất bản thành công!");
                else
                    SetErrorMessage(error ?? "Không thể xóa nhà xuất bản. Nhà xuất bản có thể đang được liên kết với sản phẩm.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting publisher");
                SetErrorFromException(ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
