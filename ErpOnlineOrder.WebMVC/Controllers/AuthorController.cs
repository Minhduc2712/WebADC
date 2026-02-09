using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Attributes;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission("CATEGORY_VIEW")]
    public class AuthorController : BaseController
    {
        private readonly IAuthorService _authorService;
        private readonly ILogger<AuthorController> _logger;

        public AuthorController(IAuthorService authorService, ILogger<AuthorController> logger)
        {
            _authorService = authorService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var authors = await _authorService.GetAllAsync();
                return View(authors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading authors");
                TempData["ErrorMessage"] = GetDetailedErrorMessage(ex);
                return View(Enumerable.Empty<Author>());
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
        public async Task<IActionResult> Create(Author model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                model.Created_by = userId;
                model.Updated_by = userId;

                await _authorService.CreateAsync(model);
                TempData["SuccessMessage"] = "Thêm tác giả thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating author");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
                return View(model);
            }
        }

        [RequirePermission("CATEGORY_UPDATE")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var author = await _authorService.GetByIdAsync(id);
                if (author == null)
                    return NotFound();

                return View(author);
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
        public async Task<IActionResult> Edit(int id, Author model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(model);

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                model.Updated_by = userId;

                await _authorService.UpdateAsync(model);
                TempData["SuccessMessage"] = "Cập nhật tác giả thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating author");
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
                await _authorService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xóa tác giả thành công!";
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
