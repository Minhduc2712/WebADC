using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.AuthorDTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Services;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission("CATEGORY_VIEW")]
    public class AuthorController : BaseController
    {
        private readonly IAuthorApiClient _authorApiClient;
        private readonly ILogger<AuthorController> _logger;

        public AuthorController(IAuthorApiClient authorApiClient, ILogger<AuthorController> logger)
        {
            _authorApiClient = authorApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var authors = await _authorApiClient.GetAllAsync();
                return View(authors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading authors");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return View(Enumerable.Empty<AuthorDto>());
            }
        }

        [RequirePermission("CATEGORY_CREATE")]
        public IActionResult Create()
        {
            return View(new CreateAuthorDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CATEGORY_CREATE")]
        public async Task<IActionResult> Create(CreateAuthorDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var created = await _authorApiClient.CreateAsync(model);
                if (created != null)
                {
                    TempData["SuccessMessage"] = "Thêm tác giả thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Thêm tác giả thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating author");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            return View(model);
        }

        [RequirePermission("CATEGORY_UPDATE")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var author = await _authorApiClient.GetByIdAsync(id);
                if (author == null)
                    return NotFound();

                var updateDto = new UpdateAuthorDto
                {
                    Id = author.Id,
                    Author_code = author.Author_code,
                    Author_name = author.Author_name,
                    Pen_name = author.Pen_name,
                    Email_author = author.Email_author,
                    Phone_number = author.Phone_number,
                    birth_date = author.birth_date,
                    death_date = author.death_date,
                    Nationality = author.Nationality,
                    Biography = author.Biography
                };
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading author for edit");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CATEGORY_UPDATE")]
        public async Task<IActionResult> Edit(int id, UpdateAuthorDto model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(model);

                var (success, error) = await _authorApiClient.UpdateAsync(id, model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật tác giả thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Cập nhật thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating author");
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
                var (success, error) = await _authorApiClient.DeleteAsync(id);
                if (success)
                    TempData["SuccessMessage"] = "Xóa tác giả thành công!";
                else
                    TempData["ErrorMessage"] = error ?? "Xóa thất bại.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting author");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
