using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.DTOs;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    public class GenerateTokenRequest
    {
        public string Username { get; set; } = string.Empty;
    }

    public class AutoLoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ApiController
    {
        private readonly IAuthService _authService;
        private readonly IRememberMeService _rememberMeService;
        private readonly IUserRepository _userRepository;
        private readonly IPermissionService _permissionService;

        public AuthController(IAuthService authService, IRememberMeService rememberMeService, IUserRepository userRepository, IPermissionService permissionService)
        {
            _authService = authService;
            _rememberMeService = rememberMeService;
            _userRepository = userRepository;
            _permissionService = permissionService;
        }

        [HttpGet("check-permissions/{userId}")]
        public async Task<IActionResult> CheckUserPermissions(int userId)
        {
            try
            {
                var result = await _authService.CheckUserPermissionsAsync(userId);
                if (result == null)
                    return NotFound(ApiResponse<object>.Fail($"User ID {userId} khong ton tai"));
                return Ok(ApiResponse<object>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }


#if DEBUG
        // WARNING: Debug-only endpoints - MUST NOT be available in production
        [HttpPost("grant-all-permissions/{userId}")]
        public async Task<IActionResult> GrantAllPermissions(int userId)
        {
            try
            {
                var result = await _authService.GrantAllPermissionsAsync(userId);
                if (result == null)
                    return NotFound(ApiResponse<object>.Fail($"User ID {userId} khong ton tai"));
                if (!result.Success)
                    return BadRequest(ApiResponse<object>.Fail(result.Message));
                return Ok(ApiResponse<object>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
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
                var result = await _authService.SeedAdminAsync(0); // Dùng ID 0 cho tài khoản hệ thống thay vì bắt buộc phải có user đăng nhập
                return Ok(ApiResponse<object>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("check-admin")]
        public async Task<IActionResult> CheckAdmin()
        {
            try
            {
                var result = await _authService.CheckAdminAsync();
                if (result == null)
                    return NotFound(ApiResponse<object>.Fail("User admin khong ton tai"));
                return Ok(ApiResponse<object>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
#endif

        [HttpPost("register-staff")]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffDto model)
        {
            // Require permission: MANAGE_USERS
            try
            {
                var result = await _authService.RegisterByAdminAsync(model);
                return Ok(ApiResponse<object>.Ok(null, "Đăng ký thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto model)
        {
            try
            {
                var result = await _authService.RegisterByCustomerAsync(model);
                return Ok(ApiResponse<object>.Ok(null, "Đăng ký thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("register-customer-finalize")]
        public async Task<IActionResult> RegisterCustomerFinalize([FromBody] FinalizeCustomerRegistrationDto model)
        {
            try
            {
                var result = await _authService.FinalizeCustomerRegistrationAsync(model);
                return Ok(ApiResponse<object>.Ok(null, "Đăng ký thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto model)
        {
            try
            {
                var response = await _authService.GetLoginResponseAsync(model);
                if (response == null)
                    return BadRequest(ApiResponse<object>.Fail("Tên đăng nhập hoặc mật khẩu không đúng."));
                return Ok(ApiResponse<LoginResponseDto>.Ok(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("generate-token")]
        public async Task<IActionResult> GenerateToken([FromBody] GenerateTokenRequest request)
        {
            try
            {
                var user = await _userRepository.GetByUsernameBasicAsync(request.Username);
                if (user == null) return BadRequest(ApiResponse<object>.Fail("User not found"));
                var token = _rememberMeService.GenerateToken(user);
                return Ok(ApiResponse<string>.Ok(token));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("auto-login")]
        public async Task<IActionResult> AutoLogin([FromBody] AutoLoginRequest request)
        {
            try
            {
                var isValid = await _rememberMeService.ValidateTokenAsync(request.Username, request.Token);
                if (!isValid) return BadRequest(ApiResponse<object>.Fail("Invalid token"));

                var user = await _userRepository.GetByUsernameAsync(request.Username);
                if (user == null || !user.Is_active || user.Is_deleted)
                    return BadRequest(ApiResponse<object>.Fail("User invalid"));

                var response = new LoginResponseDto
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.Staff?.Full_name ?? user.Customer?.Full_name ?? user.Username,
                    UserType = user.Staff != null ? "Staff" : (user.Customer != null ? "Customer" : "Admin"),
                    StaffCode = user.Staff?.Staff_code,
                    CustomerCode = user.Customer?.Customer_code,
                    Roles = user.User_roles?.Where(ur => !ur.Is_deleted && ur.Role != null && !ur.Role.Is_deleted).Select(ur => ur.Role!.Role_name).ToList() ?? new List<string>(),
                    Permissions = (await _permissionService.GetUserPermissionsAsync(user.Id)).ToList()
                };

                return Ok(ApiResponse<LoginResponseDto>.Ok(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            try
            {
                var result = await _authService.ChangePasswordAsync(model);
                return Ok(ApiResponse<object>.Ok(null, "Đổi mật khẩu thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _authService.DeleteUserAsync(id);
                return Ok(ApiResponse<object>.Ok(null, "Xóa người dùng thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            try
            {
                var exists = await _userRepository.ExistsByEmailAsync(email);
                return Ok(ApiResponse<object>.Ok(new { exists }));
            }
            catch (Exception)
            {
                return Ok(ApiResponse<object>.Ok(new { exists = false }));
            }
        }

#if DEBUG
        // WARNING: Debug-only endpoints - MUST NOT be available in production
        [HttpGet("debug-permissions/{userId}")]
        public async Task<IActionResult> DebugUserPermissions(int userId)
        {
            try
            {
                var result = await _authService.DebugUserPermissionsAsync(userId);
                if (result == null)
                    return NotFound(ApiResponse<object>.Fail($"User ID {userId} khong ton tai"));
                return Ok(ApiResponse<object>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("debug-role-permissions/{roleId}")]
        public async Task<IActionResult> DebugRolePermissions(int roleId)
        {
            try
            {
                var result = await _authService.DebugRolePermissionsAsync(roleId);
                if (result == null)
                    return NotFound(ApiResponse<object>.Fail($"Role ID {roleId} khong ton tai"));
                return Ok(ApiResponse<object>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
#endif
    }
}
