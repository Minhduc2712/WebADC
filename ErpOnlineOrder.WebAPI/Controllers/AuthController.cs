using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionService _permissionService;

        public AuthController(
            IAuthService authService, 
            IUserRepository userRepository, 
            IRoleRepository roleRepository,
            IPermissionService permissionService)
        {
            _authService = authService;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _permissionService = permissionService;
        }
        [HttpGet("check-permissions/{userId}")]
        public async Task<IActionResult> CheckUserPermissions(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = $"User ID {userId} khong ton tai" });
                }

                var permissions = await _permissionService.GetUserPermissionsAsync(userId);
                var fullPermissions = await _permissionService.GetUserFullPermissionsAsync(userId);

                return Ok(new {
                    userId = user.Id,
                    username = user.Username,
                    email = user.Email,
                    isActive = user.Is_active,
                    roles = user.User_roles?.Select(ur => new {
                        roleId = ur.Role_id,
                        roleName = ur.Role?.Role_name,
                        permissionCount = ur.Role?.Role_Permissions?.Count(rp => !rp.Is_deleted) ?? 0
                    }).ToList(),
                    allPermissionCodes = permissions.ToList(),
                    permissionCount = permissions.Count(),
                    hasStaffView = permissions.Contains("STAFF_VIEW"),
                    fullPermissions = fullPermissions
                });
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
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = $"User ID {userId} khong ton tai" });
                }

                // Kiem tra role ROLE_ADMIN
                var adminRole = await _roleRepository.GetByNameAsync("ROLE_ADMIN");
                if (adminRole == null)
                {
                    return BadRequest(new { message = "Role ROLE_ADMIN chua ton tai" });
                }

                // Gan role ROLE_ADMIN cho user
                await _userRepository.AssignRoleAsync(userId, adminRole.Id);

                return Ok(new { 
                    success = true, 
                    message = $"Da gan role ROLE_ADMIN cho user {user.Username}",
                    userId = userId,
                    roleId = adminRole.Id,
                    roleName = adminRole.Role_name
                });
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
                var newPassword = "Admin@123";
                var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                
                // Kiem tra user admin da ton tai chua
                var existingAdmin = await _userRepository.GetByUsernameAsync("admin");
                if (existingAdmin != null)
                {
                    // Cap nhat password neu user da ton tai
                    existingAdmin.Password = newHash;
                    existingAdmin.Is_active = true;
                    existingAdmin.Is_deleted = false;
                    await _userRepository.UpdateAsync(existingAdmin);
                    
                    // Verify lai
                    var verifyResult = BCrypt.Net.BCrypt.Verify(newPassword, newHash);
                    
                    return Ok(new { 
                        success = true, 
                        message = "Da cap nhat password cho admin",
                        username = "admin",
                        password = newPassword,
                        hash = newHash,
                        verified = verifyResult
                    });
                }

                // Kiem tra role ROLE_ADMIN
                var adminRole = await _roleRepository.GetByNameAsync("ROLE_ADMIN");
                if (adminRole == null)
                {
                    return BadRequest(new { message = "Role ROLE_ADMIN chua ton tai. Hay chay SeedData.sql truoc." });
                }

                // Tao user admin moi
                var user = new User
                {
                    Username = "admin",
                    Email = "admin@erponline.com",
                    Password = newHash,
                    Is_active = true,
                    Created_by = 1,
                    Created_at = DateTime.UtcNow,
                    Updated_by = 1,
                    Updated_at = DateTime.UtcNow,
                    Is_deleted = false,
                    User_roles = new List<User_role>
                    {
                        new User_role { Role_id = adminRole.Id }
                    },
                    Staff = new Staff
                    {
                        Staff_code = "ADMIN001",
                        Full_name = "Quan tri vien",
                        Phone_number = "0123456789",
                        Created_by = 1,
                        Created_at = DateTime.UtcNow,
                        Updated_by = 1,
                        Updated_at = DateTime.UtcNow,
                        Is_deleted = false
                    }
                };

                await _userRepository.AddAsync(user);

                return Ok(new { 
                    success = true, 
                    message = "Da tao user admin thanh cong",
                    username = "admin",
                    password = newPassword,
                    hash = newHash
                });
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
                var admin = await _userRepository.GetByUsernameAsync("admin");
                if (admin == null)
                {
                    return NotFound(new { message = "User admin khong ton tai" });
                }

                // Test verify password
                var testPassword = "Admin@123";
                var verifyResult = BCrypt.Net.BCrypt.Verify(testPassword, admin.Password);

                return Ok(new {
                    id = admin.Id,
                    username = admin.Username,
                    email = admin.Email,
                    isActive = admin.Is_active,
                    isDeleted = admin.Is_deleted,
                    passwordHash = admin.Password,
                    passwordHashLength = admin.Password?.Length,
                    testPassword = testPassword,
                    verifyResult = verifyResult
                });
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
                var username = await _authService.LoginAsync(model);
                if (string.IsNullOrEmpty(username))
                    return BadRequest(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });

                var user = await _userRepository.FindByIdentifierAsync(model.Identifier ?? "");
                if (user == null || !user.Is_active || user.Is_deleted)
                    return BadRequest(new { message = "Tài khoản không tồn tại hoặc đã bị vô hiệu hóa." });

                var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
                var roles = user.User_roles?
                    .Where(ur => !ur.Is_deleted && ur.Role != null && !ur.Role.Is_deleted)
                    .Select(ur => ur.Role!.Role_name)
                    .ToList() ?? new List<string>();

                var fullName = user.Staff?.Full_name ?? user.Customer?.Full_name ?? user.Username;
                var userType = user.Staff != null ? "Staff" : (user.Customer != null ? "Customer" : "");
                var staffCode = user.Staff?.Staff_code;
                var customerCode = user.Customer?.Customer_code;

                var response = new LoginResponseDto
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = fullName ?? user.Username,
                    Roles = roles,
                    Permissions = permissions.ToList(),
                    StaffCode = staffCode,
                    CustomerCode = customerCode,
                    UserType = userType
                };
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
            // Require permission: MANAGE_USERS
            try
            {
                await _userRepository.DeleteAsync(id);
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
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = $"User ID {userId} khong ton tai" });
                }

                // Debug chi tiet
                var userRolesCount = user.User_roles?.Count ?? 0;
                var activeUserRoles = user.User_roles?.Where(ur => !ur.Is_deleted).ToList() ?? new List<Domain.Models.User_role>();
                
                var roleDetails = activeUserRoles.Select(ur => new {
                    userRoleId = ur.Id,
                    roleId = ur.Role_id,
                    roleName = ur.Role?.Role_name ?? "NULL",
                    roleIsDeleted = ur.Role?.Is_deleted ?? true,
                    rolePermissionsCount = ur.Role?.Role_Permissions?.Count ?? 0,
                    activeRolePermissions = ur.Role?.Role_Permissions?
                        .Where(rp => !rp.Is_deleted && rp.Permission != null && !rp.Permission.Is_deleted)
                        .Select(rp => rp.Permission.Permission_code)
                        .ToList() ?? new List<string>()
                }).ToList();

                var permissions = await _permissionService.GetUserPermissionsAsync(userId);

                return Ok(new {
                    userId = user.Id,
                    username = user.Username,
                    email = user.Email,
                    totalUserRoles = userRolesCount,
                    activeUserRolesCount = activeUserRoles.Count,
                    roleDetails = roleDetails,
                    allPermissionCodes = permissions.ToList(),
                    permissionCount = permissions.Count(),
                    hasStaffView = permissions.Contains("STAFF_VIEW"),
                    hasProductView = permissions.Contains("PRODUCT_VIEW"),
                    hasOrderView = permissions.Contains("ORDER_VIEW")
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = ex.Message, 
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace 
                });
            }
        }
        [HttpGet("debug-role-permissions/{roleId}")]
        public async Task<IActionResult> DebugRolePermissions(int roleId)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                {
                    return NotFound(new { message = $"Role ID {roleId} khong ton tai" });
                }

                var rolePermissions = await _permissionService.GetRolePermissionsAsync(roleId);

                return Ok(new {
                    roleId = role.Id,
                    roleName = role.Role_name,
                    isDeleted = role.Is_deleted,
                    permissionCount = rolePermissions?.Permissions?.Count ?? 0,
                    permissions = rolePermissions?.Permissions?.Select(p => p.Permission_code).ToList() ?? new List<string>()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
