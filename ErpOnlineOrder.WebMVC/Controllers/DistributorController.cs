using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Services;
using ErpOnlineOrder.Application.DTOs.DistributorDTOs;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.DistributorView)]
    public class DistributorController : BaseController
    {
        private readonly IDistributorApiClient _distributorApiClient;
        private readonly ILogger<DistributorController> _logger;

        public DistributorController(IDistributorApiClient distributorApiClient, ILogger<DistributorController> logger)
        {
            _distributorApiClient = distributorApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var distributors = await _distributorApiClient.GetAllAsync();
                return View(distributors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading distributors");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return View(Enumerable.Empty<DistributorDto>());
            }
        }

        [RequirePermission(PermissionCodes.DistributorCreate)]
        public IActionResult Create()
        {
            return View(new CreateDistributorDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.DistributorCreate)]
        public async Task<IActionResult> Create(CreateDistributorDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var created = await _distributorApiClient.CreateAsync(model);
                if (created != null)
                {
                    TempData["SuccessMessage"] = "Thêm nhà phân phối thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Thêm nhà phân phối thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating distributor");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            return View(model);
        }

        [RequirePermission(PermissionCodes.DistributorUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var distributor = await _distributorApiClient.GetByIdAsync(id);
                if (distributor == null)
                    return NotFound();

                var updateDto = new UpdateDistributorDto
                {
                    Id = distributor.Id,
                    Distributor_code = distributor.Distributor_code ?? "",
                    Distributor_name = distributor.Distributor_name ?? "",
                    Distributor_address = distributor.Distributor_address ?? "",
                    Distributor_phone = distributor.Distributor_phone ?? "",
                    Distributor_email = distributor.Distributor_email ?? ""
                };
                return View(updateDto);
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
        public async Task<IActionResult> Edit(int id, UpdateDistributorDto model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(model);

                var (success, error) = await _distributorApiClient.UpdateAsync(id, model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật nhà phân phối thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Cập nhật thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating distributor");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.DistributorDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, error) = await _distributorApiClient.DeleteAsync(id);
                if (success)
                    TempData["SuccessMessage"] = "Xóa nhà phân phối thành công!";
                else
                    TempData["ErrorMessage"] = error ?? "Xóa thất bại.";
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
