using BCrypt.Net;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Security;
using ErpOnlineOrder.Application.DTOs.EmailDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Enums;
using ErpOnlineOrder.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionService _permissionService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICustomerManagementRepository _customerManagementRepository;
        private readonly IStaffRegionRuleRepository _staffRegionRuleRepository;
        private readonly IEmailQueue _emailQueue;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPermissionService permissionService,
            IPasswordHasher passwordHasher,
            ICustomerManagementRepository customerManagementRepository,
            IStaffRegionRuleRepository staffRegionRuleRepository,
            IEmailQueue emailQueue)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _permissionService = permissionService;
            _passwordHasher = passwordHasher;
            _customerManagementRepository = customerManagementRepository;
            _staffRegionRuleRepository = staffRegionRuleRepository;
            _emailQueue = emailQueue;
        }

        public async Task<bool> RegisterByAdminAsync(RegisterStaffDto dto)
        {
            int created_id = dto.Created_id;

            string username = dto.Username;

            string email = dto.Email;

            if (await _userRepository.ExistsByUsernameAsync(username)) throw new Exception("Tên đăng nhập đã tồn tại");
            if (await _userRepository.ExistsByEmailAsync(email)) throw new Exception("Email đã tồn tại");

            string roleName = dto.Role_type.ToString();

            var roleFromDb = await _roleRepository.GetByNameAsync(roleName);

            if (roleFromDb == null)
                throw new Exception($"Cấu hình lỗi: role không tồn tại.");

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
             
            if (await _userRepository.ExistsByUsernameAsync(Username)) throw new Exception("Tài khoản đã tồn tại");
            if (await _userRepository.ExistsByEmailAsync(Email)) throw new Exception("Email đã tồn tại");

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
                    Organization_information_id = dto.Organization_information_id,
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

        public async Task<bool> FinalizeCustomerRegistrationAsync(FinalizeCustomerRegistrationDto dto)
        {
            var username = dto.Account.Username.Trim();
            var email = dto.Account.Email.Trim();

            if (await _userRepository.ExistsByUsernameAsync(username))
                throw new Exception("Tài khoản đã tồn tại");

            if (await _userRepository.ExistsByEmailAsync(email))
                throw new Exception("Email đã tồn tại");

            const string roleName = "ROLE_CUSTOMER";
            var roleFromDb = await _roleRepository.GetByNameAsync(roleName);
            if (roleFromDb == null)
                throw new Exception($"Cấu hình lỗi: '{roleName}' không tồn tại trong Database.");

            var now = DateTime.UtcNow;
            var user = new User
            {
                Username = username,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Account.Password),
                User_roles = new List<User_role>
                {
                    new User_role { Role_id = roleFromDb.Id }
                },
                Is_active = true,
                Created_at = now,
                Created_by = 0,
                Updated_at = now,
                Updated_by = 0,
                Is_deleted = false,
                Customer = new Customer
                {
                    Customer_code = $"CUST-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    Full_name = dto.Personal.Full_name.Trim(),
                    Phone_number = dto.Personal.Phone_number.Trim(),
                    Address = dto.Personal.Address.Trim(),
                    Recipient_name = dto.Personal.Recipient_name,
                    Recipient_phone = dto.Personal.Recipient_phone ?? string.Empty,
                    Recipient_address = dto.Personal.Recipient_address,
                    Organization_information_id = dto.Organization.Organization_information_id,
                    Created_at = now,
                    Created_by = 0,
                    Updated_at = now,
                    Updated_by = 0,
                    Is_deleted = false
                }
            };

            await _userRepository.AddAsync(user);

            // Auto-assign managing staff based on pre-configured Staff_region_rules
            if (dto.Personal.Province_id.HasValue && user.Customer != null)
            {
                var rule = await _staffRegionRuleRepository.FindByProvinceAndWardAsync(
                    dto.Personal.Province_id.Value, dto.Personal.Ward_id);

                if (rule != null)
                {
                    await _customerManagementRepository.AddAsync(new Customer_management
                    {
                        Staff_id = rule.Staff_id,
                        Customer_id = user.Customer.Id,
                        Province_id = dto.Personal.Province_id.Value,
                        Ward_id = dto.Personal.Ward_id,
                        Created_by = 0,
                        Created_at = now,
                        Updated_by = 0,
                        Updated_at = now,
                        Is_deleted = false
                    });
                }
            }

            // Gửi email thông báo đăng ký (bất đồng bộ, không chặn luồng đăng ký)
            if (user.Customer?.Id > 0)
            {
                await _emailQueue.EnqueueAsync(new EmailMessage
                {
                    ActionType = EmailActionType.CustomerRegistrationNotification,
                    PrimaryId = user.Customer.Id
                });
            }

            return true;
        }

        public async Task<string?> LoginAsync(LoginUserDto dto)
        {
            if (string.IsNullOrEmpty(dto.Identifier)) throw new Exception("Tên đăng nhập không được để trống");

            var user = await _userRepository.GetForLoginAsync(dto.Identifier);

            if (user == null) 
            {
                throw new Exception("Sai tên đăng nhập hoặc mật khẩu");
            }

            if (!user.Is_active)
            {
                throw new Exception("Tài khoản đã bị khóa.");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                throw new Exception("Sai tên đăng nhập hoặc mật khẩu");
            }

            return user.Username;
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
        {
            string identifier = dto.Identifier ?? string.Empty;
            string OldPassword = dto.OldPassword;
            string NewPassword = dto.NewPassword;

            bool isEmail = identifier.Contains("@");

            string username = isEmail ? identifier.Split('@')[0] : identifier;

            var existingUser = isEmail ? await _userRepository.GetByEmailBasicAsync(identifier)
                : await _userRepository.GetByUsernameBasicAsync(username);

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

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userRepository.GetByEmailBasicAsync(dto.Email);
            if (user == null || !user.Is_active || user.Is_deleted) return; // Silent: không tiết lộ email tồn tại hay không

            // Tạo token ngẫu nhiên 32 bytes
            var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var hashedToken = ComputeSha256(rawToken);

            user.Password_reset_token = hashedToken;
            user.Password_reset_token_expiry = DateTime.UtcNow.AddHours(1);
            user.Updated_at = DateTime.UtcNow;
            user.Updated_by = user.Id;
            await _userRepository.UpdateAsync(user);

            // Tạo link đặt lại mật khẩu
            var resetLink = $"{dto.ResetBaseUrl.TrimEnd('/')}/Auth/ResetPassword?token={Uri.EscapeDataString(rawToken)}&email={Uri.EscapeDataString(dto.Email)}";

            await _emailQueue.EnqueueAsync(new EmailMessage
            {
                ActionType = EmailActionType.PasswordReset,
                PrimaryId = user.Id,
                Payload = resetLink
            });
        }

        public async Task<bool> ResetPasswordAsync(PasswordResetDto dto)
        {
            var user = await _userRepository.GetByEmailBasicAsync(dto.Email);
            if (user == null || !user.Is_active || user.Is_deleted)
                throw new Exception("Yêu cầu đặt lại mật khẩu không hợp lệ.");

            if (string.IsNullOrEmpty(user.Password_reset_token) || user.Password_reset_token_expiry == null)
                throw new Exception("Token đặt lại mật khẩu không tồn tại hoặc đã được sử dụng.");

            if (user.Password_reset_token_expiry < DateTime.UtcNow)
                throw new Exception("Token đặt lại mật khẩu đã hết hạn. Vui lòng yêu cầu lại.");

            var hashedSubmitted = ComputeSha256(dto.Token);
            if (!string.Equals(hashedSubmitted, user.Password_reset_token, StringComparison.Ordinal))
                throw new Exception("Token đặt lại mật khẩu không hợp lệ.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.Password_reset_token = null;
            user.Password_reset_token_expiry = null;
            user.Updated_at = DateTime.UtcNow;
            user.Updated_by = user.Id;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        private static string ComputeSha256(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
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

            var existingAdmin = await _userRepository.GetByUsernameBasicAsync("admin");
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
            var admin = await _userRepository.GetByUsernameBasicAsync("admin");
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
            var role = await _roleRepository.GetByIdBasicAsync(roleId);
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
