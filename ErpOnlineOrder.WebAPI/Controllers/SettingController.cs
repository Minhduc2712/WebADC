using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.SettingsDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.WebAPI.Attributes;
using ErpOnlineOrder.Application.DTOs;

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
            var settings = await _settingService.GetAllAsync(TryGetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(settings));
        }

        [HttpGet("{key}")]
        [RequirePermission(PermissionCodes.SettingsView)]
        public async Task<IActionResult> GetByKey(string key)
        {
            var value = await _settingService.GetAsync(key);
            if (value == null) return NotFound(ApiResponse<object>.Fail($"Setting key '{key}' không tồn tại."));
            return Ok(ApiResponse<object>.Ok(new { key, value }));
        }

        [HttpPut("{key}")]
        [RequirePermission(PermissionCodes.SettingsUpdate)]
        public async Task<IActionResult> Set(string key, [FromBody] SetSettingRequest request)
        {
            try
            {
                await _settingService.SetAsync(key, request.Value, GetCurrentUserId());
                return Ok(ApiResponse<object>.Ok(null, "Đã cập nhật."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("smtp")]
        [RequirePermission(PermissionCodes.SettingsView)]
        public async Task<IActionResult> GetSmtpSettings()
        {
            var settings = await _settingService.GetSmtpSettingsAsync();
            return Ok(ApiResponse<object>.Ok(settings));
        }

        [HttpPut("smtp")]
        [RequirePermission(PermissionCodes.SettingsUpdate)]
        public async Task<IActionResult> SaveSmtpSettings([FromBody] SmtpSettingsDto dto)
        {
            try
            {
                await _settingService.SaveSmtpSettingsAsync(dto, GetCurrentUserId());
                return Ok(ApiResponse<object>.Ok(null, "Đã lưu cấu hình SMTP."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost]
        [RequirePermission(PermissionCodes.SettingsUpdate)]
        public async Task<IActionResult> Create([FromBody] CreateSettingDto dto)
        {
            try
            {
                var key = dto.SettingKey?.Trim().ToUpperInvariant() ?? "";
                if (string.IsNullOrEmpty(key))
                    return BadRequest(ApiResponse<object>.Fail("Mã cài đặt không được để trống."));

                var created = await _settingService.CreateSettingAsync(key, dto.SettingValue ?? "", dto.Description, GetCurrentUserId());
                if (!created)
                    return BadRequest(ApiResponse<object>.Fail($"Mã cài đặt '{key}' đã tồn tại."));
                return Ok(ApiResponse<object>.Ok(null, "Đã thêm cài đặt mới."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id:int}")]
        [RequirePermission(PermissionCodes.SettingsUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSettingDto dto)
        {
            try
            {
                var updated = await _settingService.UpdateSettingAsync(id, dto.SettingValue ?? "", dto.Description, GetCurrentUserId());
                if (!updated)
                    return NotFound(ApiResponse<object>.Fail("Cài đặt không tồn tại."));
                return Ok(ApiResponse<object>.Ok(null, "Đã cập nhật."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }

    public class SetSettingRequest
    {
        public string Value { get; set; } = "";
    }
}
