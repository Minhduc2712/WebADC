using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.SettingsDTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    public class SettingController : BaseController
    {
        private readonly ISettingApiClient _settingApiClient;
        private readonly IPermissionApiClient _permissionApiClient;
        private readonly ILogger<SettingController> _logger;

        public SettingController(ISettingApiClient settingApiClient, IPermissionApiClient permissionApiClient, ILogger<SettingController> logger)
        {
            _settingApiClient = settingApiClient;
            _permissionApiClient = permissionApiClient;
            _logger = logger;
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.SettingsView)]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Cài đặt hệ thống";
            await LoadCurrentUserPermissions();
            try
            {
                var smtp = await _settingApiClient.GetSmtpSettingsAsync();
                var allSettings = (await _settingApiClient.GetAllSettingsAsync())?.ToList();
                var model = new SettingIndexViewModel
                {
                    Smtp = smtp ?? new SmtpSettingsDto(),
                    AllSettings = allSettings ?? new List<SystemSettingDto>(),
                    WarehouseEmailSetting = allSettings?.FirstOrDefault(s => s.SettingKey == "WAREHOUSE_NOTIFY_EMAIL")
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải cài đặt");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(new SettingIndexViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.SettingsUpdate)]
        public async Task<IActionResult> SaveSmtp(SmtpSettingsDto model)
        {
            try
            {
                var (success, error) = await _settingApiClient.SaveSmtpSettingsAsync(model);
                if (success)
                {
                    SetSuccessMessage("Đã lưu cấu hình SMTP thành công!");
                    return RedirectToAction(nameof(Index));
                }
                SetErrorMessage(error ?? "Không thể lưu cấu hình.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu cài đặt SMTP");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }
            await LoadCurrentUserPermissions();
            var allSettings = await _settingApiClient.GetAllSettingsAsync();
            return View("Index", new SettingIndexViewModel { Smtp = model, AllSettings = allSettings ?? new List<SystemSettingDto>() });
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.SettingsUpdate)]
        public IActionResult CreateSetting()
        {
            return View(new CreateSettingDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.SettingsUpdate)]
        public async Task<IActionResult> CreateSetting(CreateSettingDto model)
        {
            if (string.IsNullOrWhiteSpace(model?.SettingKey))
            {
                ModelState.AddModelError("SettingKey", "Mã cài đặt không được để trống");
                return View(model ?? new CreateSettingDto());
            }

            try
            {
                var (success, error) = await _settingApiClient.CreateSettingAsync(model);
                if (success)
                {
                    SetSuccessMessage("Đã thêm cài đặt mới!");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Mã cài đặt có thể đã tồn tại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm cài đặt");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            return View(model);
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.SettingsUpdate)]
        public async Task<IActionResult> EditSetting(int id)
        {
            var allSettings = await _settingApiClient.GetAllSettingsAsync();
            var setting = allSettings?.FirstOrDefault(s => s.Id == id);
            if (setting == null)
            {
                SetErrorMessage("Không tìm thấy cài đặt.");
                return RedirectToAction(nameof(Index));
            }

            var model = new UpdateSettingDto { SettingValue = setting.SettingValue, Description = setting.Description };
            ViewBag.SettingId = id;
            ViewBag.SettingKey = setting.SettingKey;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.SettingsUpdate)]
        public async Task<IActionResult> EditSetting(int id, UpdateSettingDto model)
        {
            try
            {
                var (success, error) = await _settingApiClient.UpdateSettingAsync(id, model);
                if (success)
                {
                    SetSuccessMessage("Đã cập nhật cài đặt!");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error ?? "Không thể cập nhật cài đặt. Vui lòng kiểm tra giá trị nhập và kiểu dữ liệu của cấu hình.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật cài đặt");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }
            var allSettings = await _settingApiClient.GetAllSettingsAsync();
            var setting = allSettings?.FirstOrDefault(s => s.Id == id);
            ViewBag.SettingId = id;
            ViewBag.SettingKey = setting?.SettingKey ?? "";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.SettingsUpdate)]
        public async Task<IActionResult> SaveWarehouseSettings(string email, int? settingId)
        {
            try
            {
                if (settingId.HasValue)
                    await _settingApiClient.UpdateSettingAsync(settingId.Value, new UpdateSettingDto { SettingValue = email ?? "", Description = "Email nhận thông báo xuất kho" });
                else
                    await _settingApiClient.CreateSettingAsync(new CreateSettingDto { SettingKey = "WAREHOUSE_NOTIFY_EMAIL", SettingValue = email ?? "", Description = "Email nhận thông báo xuất kho" });
                SetSuccessMessage("Đã lưu cấu hình kho thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu cài đặt kho");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCurrentUserPermissions()
        {
            try
            {
                var roles = HttpContext.Session.GetString("Roles") ?? "";
                if (roles.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains("ROLE_ADMIN"))
                {
                    ViewBag.CanUpdate = true;
                    return;
                }
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                var permissions = await _permissionApiClient.GetUserPermissionCodesAsync(userId);
                ViewBag.CanUpdate = permissions?.Contains(PermissionCodes.SettingsUpdate) ?? false;
            }
            catch
            {
                ViewBag.CanUpdate = false;
            }
        }
    }
}
