using BCrypt.Net;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Security;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Enums;
using ErpOnlineOrder.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionService _permissionService;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPermissionService permissionService,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _permissionService = permissionService;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> RegisterByAdminAsync(RegisterStaffDto dto)
        {
            int created_id = dto.Created_id;

            string username = dto.Username;

            string email = dto.Email;

            var exisitingStaffByUsername = await _userRepository.GetByUsernameAsync(username);
            if (exisitingStaffByUsername != null) throw new Exception("Tên đăng nhập đã tồn tại");
            var existingStaffByEmail = await _userRepository.GetByEmailAsync(email);
            if (existingStaffByEmail != null) throw new Exception("Email đã tồn tại");

            string roleName = dto.Role_type.ToString();

            var roleFromDb = await _roleRepository.GetByNameAsync(roleName);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                User_roles = new List<User_role>
                {
                    new User_role { Role_id = roleFromDb.Id }
                },
                Is_active = true,
                Created_by = created_id,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false,
                Staff = new Staff
                {
                    Staff_code = $"STAFF-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    Full_name = dto.Full_name,
                    Phone_number = dto.Phone_number, 
                    Created_by = created_id,
                    Created_at = DateTime.UtcNow,
                    Updated_at = DateTime.UtcNow,
                    Is_deleted = false
                }
            };

            await _userRepository.AddAsync(user);
            return true;
        }

        public async Task<bool> RegisterByCustomerAsync(RegisterCustomerDto dto)
        {

            string Username = dto.Username;

            string Email = dto.Email;
             
            var existingUsername = await _userRepository.GetByUsernameAsync(Username);
            if(existingUsername != null) throw new Exception("Tài khoản đã tồn tại");
            var existingEmail = await _userRepository.GetByEmailAsync(Email);
            if (existingEmail != null) throw new Exception("Email đã tồn tại");

            string roleName = "ROLE_CUSTOMER";

            //string roleName = RoleType.ROLE_CUSTOMER.ToString();

            var roleFromDb = await _roleRepository.GetByNameAsync(roleName);

            if (roleFromDb == null)
            {
                throw new Exception($"Cấu hình lỗi: '{roleName}' không tồn tại trong Database.");
            }

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                User_roles = new List<User_role>
                        {
                            new User_role { Role_id = roleFromDb.Id }
                        },
                Is_active = true,
                Created_at = DateTime.UtcNow,
                Created_by = 0,
                Updated_at = DateTime.UtcNow,
                Updated_by = 0,
                Is_deleted = false,
                Customer = new Customer
                {
                    Customer_code = $"CUST-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    Full_name = null,
                    Phone_number = null,
                    Address = null,
                    Created_at = DateTime.UtcNow,
                    Created_by = 0,
                    Updated_at = DateTime.UtcNow,
                    Updated_by = 0,
                    Is_deleted = false
                }
            };

            await _userRepository.AddAsync(user);
            return true;
        }

        public async Task<string?> LoginAsync(LoginUserDto dto)
        {
            string identifier = dto.Identifier;
            string password = dto.Password;

            bool isEmail = identifier.Contains("@");

            string username = isEmail ? identifier.Split('@')[0] : identifier;

            var existingUser = isEmail ? await _userRepository.GetByEmailAsync(identifier)
                : await _userRepository.GetByUsernameAsync(username);

            if (existingUser == null) throw new Exception("Sai tên đăng nhập hoặc mật khẩu");

            if (existingUser != null && BCrypt.Net.BCrypt.Verify(password, existingUser.Password))
            {
                return existingUser.Username;
            }
            else
            {
                throw new Exception("Sai tên đăng nhập hoặc mật khẩu");
            }
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
        {
            string identifier = dto.Identifier;
            string OldPassword = dto.OldPassword;
            string NewPassword = dto.NewPassword;

            bool isEmail = identifier.Contains("@");

            string username = isEmail ? identifier.Split('@')[0] : identifier;

            var existingUser = isEmail ? await _userRepository.GetByEmailAsync(identifier)
                : await _userRepository.GetByUsernameAsync(username);

            if (existingUser == null) throw new Exception("Người dùng không tồn tại");

            if (!BCrypt.Net.BCrypt.Verify(OldPassword, existingUser.Password))
            {
                throw new Exception("Mật khẩu cũ không đúng");
            }
            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            existingUser.Updated_at = DateTime.UtcNow;
            existingUser.Updated_by = existingUser.Id;
            await _userRepository.UpdateAsync(existingUser);
            return true;
        }

        public async Task<LoginResponseDto?> GetLoginResponseAsync(LoginUserDto dto)
        {
            var username = await LoginAsync(dto);
            if (string.IsNullOrEmpty(username)) return null;

            var user = await _userRepository.FindByIdentifierAsync(dto.Identifier ?? "");
            if (user == null || !user.Is_active || user.Is_deleted) return null;

            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
            var roles = user.User_roles?
                .Where(ur => !ur.Is_deleted && ur.Role != null && !ur.Role.Is_deleted)
                .Select(ur => ur.Role!.Role_name)
                .ToList() ?? new List<string>();

            var fullName = user.Staff?.Full_name ?? user.Customer?.Full_name ?? user.Username;
            var userType = user.Staff != null ? "Staff" : (user.Customer != null ? "Customer" : "");

            return new LoginResponseDto
            {
                UserId = user.Id,
                Username = user.Username ?? "",
                Email = user.Email,
                FullName = fullName ?? user.Username ?? "",
                Roles = roles,
                Permissions = permissions.ToList(),
                StaffCode = user.Staff?.Staff_code,
                CustomerCode = user.Customer?.Customer_code,
                UserType = userType
            };
        }

        public async Task<CheckUserPermissionsResultDto?> CheckUserPermissionsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            var fullPermissions = await _permissionService.GetUserFullPermissionsAsync(userId);

            var roles = user.User_roles?
                .Where(ur => ur?.Role != null)
                .Select(ur => new RoleInfoDto
                {
                    RoleId = ur!.Role_id,
                    RoleName = ur.Role?.Role_name,
                    PermissionCount = ur.Role?.Role_Permissions?.Count(rp => !rp.Is_deleted) ?? 0
                })
                .ToList() ?? new List<RoleInfoDto>();

            return new CheckUserPermissionsResultDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.Is_active,
                Roles = roles,
                AllPermissionCodes = permissions.ToList(),
                PermissionCount = permissions.Count(),
                HasStaffView = permissions.Contains("STAFF_VIEW"),
                FullPermissions = fullPermissions
            };
        }

        public async Task<GrantAllPermissionsResultDto?> GrantAllPermissionsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            var adminRole = await _roleRepository.GetByNameAsync("ROLE_ADMIN");
            if (adminRole == null) return new GrantAllPermissionsResultDto
            {
                Success = false,
                Message = "Role ROLE_ADMIN chua ton tai",
                UserId = userId,
                RoleId = 0,
                RoleName = null
            };

            await _userRepository.AssignRoleAsync(userId, adminRole.Id);

            return new GrantAllPermissionsResultDto
            {
                Success = true,
                Message = $"Da gan role ROLE_ADMIN cho user {user.Username}",
                UserId = userId,
                RoleId = adminRole.Id,
                RoleName = adminRole.Role_name
            };
        }

        public async Task<SeedAdminResultDto> SeedAdminAsync(int createdBy)
        {
            var newPassword = "Admin@123";
            var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            var existingAdmin = await _userRepository.GetByUsernameAsync("admin");
            if (existingAdmin != null)
            {
                existingAdmin.Password = newHash;
                existingAdmin.Is_active = true;
                existingAdmin.Is_deleted = false;
                await _userRepository.UpdateAsync(existingAdmin);

                return new SeedAdminResultDto
                {
                    Success = true,
                    Message = "Da cap nhat password cho admin",
                    Username = "admin",
                    Password = newPassword,
                    Hash = newHash,
                    Verified = BCrypt.Net.BCrypt.Verify(newPassword, newHash)
                };
            }

            var adminRole = await _roleRepository.GetByNameAsync("ROLE_ADMIN");
            if (adminRole == null)
                throw new InvalidOperationException("Role ROLE_ADMIN chua ton tai. Hay chay SeedData.sql truoc.");

            var user = new User
            {
                Username = "admin",
                Email = "admin@erponline.com",
                Password = newHash,
                Is_active = true,
                Created_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_by = createdBy,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false,
                User_roles = new List<User_role> { new User_role { Role_id = adminRole.Id } },
                Staff = new Staff
                {
                    Staff_code = "ADMIN001",
                    Full_name = "Quan tri vien",
                    Phone_number = "0123456789",
                    Created_by = createdBy,
                    Created_at = DateTime.UtcNow,
                    Updated_by = createdBy,
                    Updated_at = DateTime.UtcNow,
                    Is_deleted = false
                }
            };

            await _userRepository.AddAsync(user);

            return new SeedAdminResultDto
            {
                Success = true,
                Message = "Da tao user admin thanh cong",
                Username = "admin",
                Password = newPassword,
                Hash = newHash,
                Verified = true
            };
        }

        public async Task<CheckAdminResultDto?> CheckAdminAsync()
        {
            var admin = await _userRepository.GetByUsernameAsync("admin");
            if (admin == null) return null;

            var testPassword = "Admin@123";
            return new CheckAdminResultDto
            {
                Id = admin.Id,
                Username = admin.Username,
                Email = admin.Email,
                IsActive = admin.Is_active,
                IsDeleted = admin.Is_deleted,
                PasswordHash = admin.Password,
                PasswordHashLength = admin.Password?.Length,
                TestPassword = testPassword,
                VerifyResult = BCrypt.Net.BCrypt.Verify(testPassword, admin.Password)
            };
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            await _userRepository.DeleteAsync(id);
            return true;
        }

        public async Task<DebugUserPermissionsResultDto?> DebugUserPermissionsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            var activeUserRoles = user.User_roles?.Where(ur => !ur.Is_deleted).ToList() ?? new List<User_role>();
            var roleDetails = activeUserRoles.Select(ur => new DebugRoleDetailDto
            {
                UserRoleId = ur.Id,
                RoleId = ur.Role_id,
                RoleName = ur.Role?.Role_name ?? "NULL",
                RoleIsDeleted = ur.Role?.Is_deleted ?? true,
                RolePermissionsCount = ur.Role?.Role_Permissions?.Count ?? 0,
                ActiveRolePermissions = ur.Role?.Role_Permissions?
                    .Where(rp => !rp.Is_deleted && rp.Permission != null && !rp.Permission.Is_deleted)
                    .Select(rp => rp.Permission!.Permission_code)
                    .ToList() ?? new List<string>()
            }).ToList();

            var permissions = await _permissionService.GetUserPermissionsAsync(userId);

            return new DebugUserPermissionsResultDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                TotalUserRoles = user.User_roles?.Count ?? 0,
                ActiveUserRolesCount = activeUserRoles.Count,
                RoleDetails = roleDetails,
                AllPermissionCodes = permissions.ToList(),
                PermissionCount = permissions.Count(),
                HasStaffView = permissions.Contains("STAFF_VIEW"),
                HasProductView = permissions.Contains("PRODUCT_VIEW"),
                HasOrderView = permissions.Contains("ORDER_VIEW")
            };
        }

        public async Task<DebugRolePermissionsResultDto?> DebugRolePermissionsAsync(int roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null) return null;

            var rolePermissions = await _permissionService.GetRolePermissionsAsync(roleId);

            return new DebugRolePermissionsResultDto
            {
                RoleId = role.Id,
                RoleName = role.Role_name,
                IsDeleted = role.Is_deleted,
                PermissionCount = rolePermissions?.Permissions?.Count ?? 0,
                Permissions = rolePermissions?.Permissions?.Select(p => p.Permission_code).ToList() ?? new List<string>()
            };
        }
    }
}
