using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.SettingsDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.WebAPI.Attributes;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingController : ApiController
    {
        private readonly ISettingService _settingService;

        public SettingController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.SettingsView)]
        public async Task<IActionResult> GetAll()
        {
            var settings = await _settingService.GetAllAsync();
            return Ok(settings);
        }

        [HttpGet("{key}")]
        [RequirePermission(PermissionCodes.SettingsView)]
        public async Task<IActionResult> GetByKey(string key)
        {
            var value = await _settingService.GetAsync(key);
            if (value == null) return NotFound(new { message = $"Setting key '{key}' không tồn tại." });
            return Ok(new { key, value });
        }

        [HttpPut("{key}")]
        [RequirePermission(PermissionCodes.SettingsUpdate)]
        public async Task<IActionResult> Set(string key, [FromBody] SetSettingRequest request)
        {
            try
            {
                await _settingService.SetAsync(key, request.Value, GetCurrentUserId());
                return Ok(new { success = true, message = "Đã cập nhật." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("smtp")]
        [RequirePermission(PermissionCodes.SettingsView)]
        public async Task<IActionResult> GetSmtpSettings()
        {
            var settings = await _settingService.GetSmtpSettingsAsync();
            return Ok(settings);
        }

        [HttpPut("smtp")]
        [RequirePermission(PermissionCodes.SettingsUpdate)]
        public async Task<IActionResult> SaveSmtpSettings([FromBody] SmtpSettingsDto dto)
        {
            try
            {
                await _settingService.SaveSmtpSettingsAsync(dto, GetCurrentUserId());
                return Ok(new { success = true, message = "Đã lưu cấu hình SMTP." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class SetSettingRequest
    {
        public string Value { get; set; } = "";
    }
}
