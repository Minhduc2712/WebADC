using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("check-permissions/{userId}")]
        public async Task<IActionResult> CheckUserPermissions(int userId)
        {
            try
            {
                var result = await _authService.CheckUserPermissionsAsync(userId);
                if (result == null)
                    return NotFound(new { message = $"User ID {userId} khong ton tai" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost("grant-all-permissions/{userId}")]
        public async Task<IActionResult> GrantAllPermissions(int userId)
        {
            try
            {
                var result = await _authService.GrantAllPermissionsAsync(userId);
                if (result == null)
                    return NotFound(new { message = $"User ID {userId} khong ton tai" });
                if (!result.Success)
                    return BadRequest(new { message = result.Message });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("hash")]
        public IActionResult GetHash([FromQuery] string password = "Admin@123")
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var verify = BCrypt.Net.BCrypt.Verify(password, hash);
            return Ok(new { 
                password = password,
                hash = hash,
                verified = verify
            });
        }
        [HttpPost("seed-admin")]
        public async Task<IActionResult> SeedAdmin()
        {
            try
            {
                var result = await _authService.SeedAdminAsync(GetCurrentUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("check-admin")]
        public async Task<IActionResult> CheckAdmin()
        {
            try
            {
                var result = await _authService.CheckAdminAsync();
                if (result == null)
                    return NotFound(new { message = "User admin khong ton tai" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register-staff")]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffDto model)
        {
            // Require permission: MANAGE_USERS
            try
            {
                var result = await _authService.RegisterByAdminAsync(model);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto model)
        {
            try
            {
                var result = await _authService.RegisterByCustomerAsync(model);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto model)
        {
            try
            {
                var response = await _authService.GetLoginResponseAsync(model);
                if (response == null)
                    return BadRequest(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
                return Ok(new { success = true, user = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            try
            {
                var result = await _authService.ChangePasswordAsync(model);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _authService.DeleteUserAsync(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("debug-permissions/{userId}")]
        public async Task<IActionResult> DebugUserPermissions(int userId)
        {
            try
            {
                var result = await _authService.DebugUserPermissionsAsync(userId);
                if (result == null)
                    return NotFound(new { message = $"User ID {userId} khong ton tai" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, innerException = ex.InnerException?.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("debug-role-permissions/{roleId}")]
        public async Task<IActionResult> DebugRolePermissions(int roleId)
        {
            try
            {
                var result = await _authService.DebugRolePermissionsAsync(roleId);
                if (result == null)
                    return NotFound(new { message = $"Role ID {roleId} khong ton tai" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
