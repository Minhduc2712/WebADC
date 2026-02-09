using ErpOnlineOrder.Application.DTOs.PermissionDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IUserPermissionRepository _userPermissionRepository;

        public PermissionService(
            IUserRepository userRepository,
            IPermissionRepository permissionRepository,
            IRoleRepository roleRepository,
            IRolePermissionRepository rolePermissionRepository,
            IUserPermissionRepository userPermissionRepository)
        {
            _userRepository = userRepository;
            _permissionRepository = permissionRepository;
            _roleRepository = roleRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _userPermissionRepository = userPermissionRepository;
        }

        #region Kiểm tra quyền

        public async Task<bool> HasPermissionAsync(int userId, string permissionCode)
        {
            // Kiểm tra quyền từ Role
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            var hasRolePermission = user.User_roles.Any(ur =>
                !ur.Is_deleted &&
                ur.Role != null &&
                !ur.Role.Is_deleted &&
                ur.Role.Role_Permissions.Any(rp => 
                    !rp.Is_deleted && 
                    rp.Permission != null &&
                    rp.Permission.Permission_code == permissionCode && 
                    !rp.Permission.Is_deleted));

            if (hasRolePermission) return true;

            // Kiểm tra quyền trực tiếp
            var directPermissions = await _userPermissionRepository.GetByUserIdAsync(userId);
            return directPermissions.Any(dp => 
                dp.Permission != null &&
                dp.Permission.Permission_code == permissionCode && 
                !dp.Permission.Is_deleted);
        }

        public async Task<bool> HasAnyPermissionAsync(int userId, params string[] permissionCodes)
        {
            var userPermissions = await GetUserPermissionsAsync(userId);
            return permissionCodes.Any(pc => userPermissions.Contains(pc));
        }

        public async Task<bool> HasAllPermissionsAsync(int userId, params string[] permissionCodes)
        {
            var userPermissions = await GetUserPermissionsAsync(userId);
            return permissionCodes.All(pc => userPermissions.Contains(pc));
        }
        public async Task<bool> IsAdminAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            return user.User_roles.Any(ur =>
                !ur.Is_deleted &&
                ur.Role != null &&
                !ur.Role.Is_deleted &&
                ur.Role.Role_name == "ROLE_ADMIN");
        }

        #endregion

        #region Lay danh sach quyen

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return Enumerable.Empty<string>();

            // ROLE_ADMIN có toàn quyền - trả về TẤT CẢ permissions trong hệ thống
            var isAdmin = user.User_roles.Any(ur =>
                !ur.Is_deleted && ur.Role != null && !ur.Role.Is_deleted &&
                ur.Role.Role_name == "ROLE_ADMIN");

            if (isAdmin)
            {
                var allPermissions = await _permissionRepository.GetAllAsync();
                return allPermissions.Select(p => p.Permission_code).Distinct();
            }

            // Quyền từ Role
            var rolePermissions = user.User_roles
                .Where(ur => !ur.Is_deleted && ur.Role != null && !ur.Role.Is_deleted)
                .SelectMany(ur => ur.Role.Role_Permissions ?? Enumerable.Empty<Role_permission>())
                .Where(rp => !rp.Is_deleted && rp.Permission != null && !rp.Permission.Is_deleted)
                .Select(rp => rp.Permission.Permission_code)
                .ToList();

            // Quyền trực tiếp
            var directPermissions = await _userPermissionRepository.GetByUserIdAsync(userId);
            var directPermissionCodes = directPermissions
                .Where(dp => dp.Permission != null && !dp.Permission.Is_deleted)
                .Select(dp => dp.Permission.Permission_code)
                .ToList();

            // Gộp và loại trùng
            return rolePermissions.Union(directPermissionCodes).Distinct();
        }

        public async Task<UserPermissionDto?> GetUserPermissionDetailsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            // Quyền từ Role
            var rolePermissions = user.User_roles
                .Where(ur => !ur.Is_deleted && ur.Role != null && !ur.Role.Is_deleted)
                .SelectMany(ur => ur.Role.Role_Permissions ?? Enumerable.Empty<Role_permission>())
                .Where(rp => !rp.Is_deleted && rp.Permission != null && !rp.Permission.Is_deleted)
                .Select(rp => rp.Permission.Permission_code)
                .ToList();

            // Quyền trực tiếp
            var directPermissions = await _userPermissionRepository.GetByUserIdAsync(userId);
            var directPermissionCodes = directPermissions
                .Where(dp => dp.Permission != null && !dp.Permission.Is_deleted)
                .Select(dp => dp.Permission.Permission_code)
                .ToList();

            // Gộp tất cả
            var allPermissions = rolePermissions.Union(directPermissionCodes).Distinct().ToList();

            var roles = user.User_roles
                .Where(ur => !ur.Is_deleted && ur.Role != null && !ur.Role.Is_deleted)
                .Select(ur => ur.Role.Role_name)
                .ToList();

            var fullName = user.Staff?.Full_name ?? user.Customer?.Full_name ?? user.Username;

            return new UserPermissionDto
            {
                User_id = user.Id,
                Username = user.Username,
                Full_name = fullName,
                Roles = roles,
                Permissions = allPermissions
            };
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _permissionRepository.GetAllAsync();
            return permissions.Select(p => 
            {
                // Parse Module_name và Action_name từ Permission_code (format: MODULE_ACTION)
                var parts = p.Permission_code.Split('_');
                var moduleName = parts.Length > 0 ? GetModuleDisplayName(parts[0]) : p.Permission_code;
                var actionName = parts.Length > 1 ? GetActionDisplayName(string.Join("_", parts.Skip(1))) : "";

                return new PermissionDto
                {
                    Id = p.Id,
                    Permission_code = p.Permission_code,
                    Module_name = moduleName,
                    Action_name = actionName
                };
            });
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsByModuleAsync(string moduleCode)
        {
            var permissions = await _permissionRepository.GetAllAsync();
            return permissions
                .Where(p => p.Permission_code.StartsWith(moduleCode + "_"))
                .Select(p =>
                {
                    var parts = p.Permission_code.Split('_');
                    var actionName = parts.Length > 1 ? GetActionDisplayName(string.Join("_", parts.Skip(1))) : "";
                    return new PermissionDto
                    {
                        Id = p.Id,
                        Permission_code = p.Permission_code,
                        Module_name = GetModuleDisplayName(moduleCode),
                        Action_name = actionName
                    };
                });
        }
        private static string GetModuleDisplayName(string moduleCode)
        {
            return moduleCode.ToUpper() switch
            {
                "PRODUCT" => "Sản phẩm",
                "CATEGORY" => "Danh mục",
                "REGION" => "Vùng miền",
                "PROVINCE" => "Tỉnh/Thành",
                "ORGANIZATION" => "Tổ chức",
                "CUSTOMER" => "Khách hàng",
                "ORDER" => "Đơn hàng",
                "INVOICE" => "Hóa đơn",
                "WAREHOUSE" => "Kho hàng",
                "STAFF" => "Nhân sự",
                "REPORT" => "Báo cáo",
                "DISTRIBUTOR" => "Nhà phân phối",
                "ROLE" => "Phân quyền",
                "PERMISSION" => "Quyền hạn",
                _ => moduleCode
            };
        }
        private static string GetActionDisplayName(string actionCode)
        {
            return actionCode.ToUpper() switch
            {
                "VIEW" => "Xem",
                "CREATE" => "Thêm mới",
                "UPDATE" => "Cập nhật",
                "DELETE" => "Xóa",
                "EXPORT" => "Xuất file",
                "IMPORT" => "Nhập file",
                "APPROVE" => "Duyệt",
                "REJECT" => "Từ chối",
                "ASSIGN" => "Phân quyền",
                _ => actionCode
            };
        }

        #endregion

        #region Quản lý Permission (CRUD)

        public async Task<PermissionDto?> GetPermissionByIdAsync(int id)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null) return null;

            var parts = permission.Permission_code.Split('_');
            return new PermissionDto
            {
                Id = permission.Id,
                Permission_code = permission.Permission_code,
                Module_name = parts.Length > 0 ? GetModuleDisplayName(parts[0]) : permission.Permission_code,
                Action_name = parts.Length > 1 ? GetActionDisplayName(string.Join("_", parts.Skip(1))) : ""
            };
        }

        public async Task<bool> CreatePermissionAsync(string permissionCode, int createdBy)
        {
            // Kiểm tra trùng
            var existing = await _permissionRepository.GetByCodeAsync(permissionCode);
            if (existing != null) return false;

            var permission = new Permission
            {
                Permission_code = permissionCode.ToUpper(),
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.Now,
                Updated_at = DateTime.Now,
                Is_deleted = false
            };

            await _permissionRepository.AddAsync(permission);
            return true;
        }

        public async Task<bool> UpdatePermissionAsync(int id, string permissionCode, int updatedBy)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null) return false;

            permission.Permission_code = permissionCode.ToUpper();
            permission.Updated_by = updatedBy;
            permission.Updated_at = DateTime.Now;

            await _permissionRepository.UpdateAsync(permission);
            return true;
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null) return false;

            await _permissionRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> IsPermissionCodeExistsAsync(string permissionCode, int? excludeId = null)
        {
            var existing = await _permissionRepository.GetByCodeAsync(permissionCode.ToUpper());
            if (existing == null) return false;
            if (excludeId.HasValue && existing.Id == excludeId.Value) return false;
            return true;
        }

        #endregion

        #region Quản lý Role

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Role_name = r.Role_name,
                Permission_count = r.Role_Permissions.Count(rp => !rp.Is_deleted),
                Created_at = r.Created_at
            });
        }

        public async Task<RolePermissionDto?> GetRolePermissionsAsync(int roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null) return null;

            var rolePermissions = await _rolePermissionRepository.GetByRoleIdAsync(roleId);

            return new RolePermissionDto
            {
                Role_id = role.Id,
                Role_name = role.Role_name,
                Permissions = rolePermissions.Select(rp => 
                {
                    var parts = rp.Permission.Permission_code.Split('_');
                    return new PermissionDto
                    {
                        Id = rp.Permission.Id,
                        Permission_code = rp.Permission.Permission_code,
                        Module_name = parts.Length > 0 ? GetModuleDisplayName(parts[0]) : rp.Permission.Permission_code,
                        Action_name = parts.Length > 1 ? GetActionDisplayName(string.Join("_", parts.Skip(1))) : ""
                    };
                }).ToList()
            };
        }

        public async Task<bool> CreateRoleAsync(CreateRoleDto dto, int createdBy)
        {
            var role = new Role
            {
                Role_name = dto.Role_name,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.Now,
                Updated_at = DateTime.Now,
                Is_deleted = false
            };

            var createdRole = await _roleRepository.AddAsync(role);

            if (dto.Permission_ids != null && dto.Permission_ids.Any())
            {
                var rolePermissions = dto.Permission_ids.Select(pid => new Role_permission
                {
                    RoleId = createdRole.Id,
                    PermissionId = pid,
                    Created_by = createdBy,
                    Created_at = DateTime.Now,
                    Updated_by = createdBy,
                    Updated_at = DateTime.Now,
                    Is_deleted = false
                });

                await _rolePermissionRepository.AddRangeAsync(rolePermissions);
            }

            return true;
        }

        public async Task<bool> UpdateRoleAsync(UpdateRoleDto dto, int updatedBy)
        {
            var role = await _roleRepository.GetByIdAsync(dto.Id);
            if (role == null) return false;

            role.Role_name = dto.Role_name;
            role.Updated_by = updatedBy;
            role.Updated_at = DateTime.Now;

            await _roleRepository.UpdateAsync(role);
            return true;
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            await _roleRepository.DeleteAsync(roleId);
            return true;
        }

        #endregion

        #region Gan quyen cho Role

        public async Task<bool> AssignPermissionsToRoleAsync(AssignPermissionsToRoleDto dto)
        {
            await _rolePermissionRepository.DeleteByRoleIdAsync(dto.Role_id);

            if (dto.Permission_ids.Any())
            {
                var rolePermissions = dto.Permission_ids.Select(pid => new Role_permission
                {
                    RoleId = dto.Role_id,
                    PermissionId = pid,
                    Created_by = 0,
                    Created_at = DateTime.Now,
                    Updated_by = 0,
                    Updated_at = DateTime.Now,
                    Is_deleted = false
                });

                await _rolePermissionRepository.AddRangeAsync(rolePermissions);
            }

            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
        {
            var rolePermissions = await _rolePermissionRepository.GetByRoleIdAsync(roleId);
            var rp = rolePermissions.FirstOrDefault(x => x.PermissionId == permissionId);
            
            if (rp != null)
            {
                await _rolePermissionRepository.DeleteAsync(rp.Id);
            }

            return true;
        }

        #endregion

        #region Gan Role cho User

        public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
        {
            return await _userRepository.AssignRoleAsync(userId, roleId);
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
        {
            return await _userRepository.RemoveRoleAsync(userId, roleId);
        }

        #endregion

        #region Gan quyen TRUC TIEP cho User

        public async Task<IEnumerable<UserDirectPermissionDto>> GetUserDirectPermissionsAsync(int userId)
        {
            var userPermissions = await _userPermissionRepository.GetByUserIdAsync(userId);
            
            return userPermissions.Select(up => new UserDirectPermissionDto
            {
                Id = up.Id,
                UserId = up.UserId,
                PermissionId = up.PermissionId,
                Permission_code = up.Permission.Permission_code,
                Module_name = "",
                Action_name = "",
                GrantedAt = up.GrantedAt,
                ExpiresAt = up.ExpiresAt,
                Note = up.Note
            });
        }

        public async Task<UserFullPermissionDto?> GetUserFullPermissionsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            var fullName = user.Staff?.Full_name ?? user.Customer?.Full_name ?? user.Username;

            // Quyền từ Role
            var rolePermissionSummaries = user.User_roles
                .Where(ur => !ur.Is_deleted && ur.Role != null && !ur.Role.Is_deleted)
                .Select(ur => new RolePermissionSummaryDto
                {
                    Role_id = ur.Role.Id,
                    Role_name = ur.Role.Role_name,
                    Permission_codes = (ur.Role.Role_Permissions ?? Enumerable.Empty<Role_permission>())
                        .Where(rp => !rp.Is_deleted && rp.Permission != null && !rp.Permission.Is_deleted)
                        .Select(rp => rp.Permission.Permission_code)
                        .ToList()
                })
                .ToList();

            // Quyen truc tiep
            var directPermissions = await GetUserDirectPermissionsAsync(userId);

            // Gop tat ca permission codes
            var allFromRoles = rolePermissionSummaries.SelectMany(r => r.Permission_codes);
            var allFromDirect = directPermissions.Where(d => d.IsValid).Select(d => d.Permission_code);
            var allPermissionCodes = allFromRoles.Union(allFromDirect).Distinct().ToList();

            return new UserFullPermissionDto
            {
                User_id = user.Id,
                Username = user.Username,
                Full_name = fullName,
                RolePermissions = rolePermissionSummaries,
                DirectPermissions = directPermissions.ToList(),
                AllPermissionCodes = allPermissionCodes
            };
        }

        public async Task<bool> AssignPermissionsToUserAsync(AssignPermissionsToUserDto dto, int grantedBy)
        {
            // Xoa quyen cu
            await _userPermissionRepository.DeleteByUserIdAsync(dto.User_id);

            // Them quyen moi
            if (dto.Permission_ids.Any())
            {
                var userPermissions = dto.Permission_ids.Select(pid => new User_permission
                {
                    UserId = dto.User_id,
                    PermissionId = pid,
                    GrantedBy = grantedBy,
                    GrantedAt = DateTime.Now,
                    ExpiresAt = dto.ExpiresAt,
                    Note = dto.Note,
                    IsDeleted = false
                });

                await _userPermissionRepository.AddRangeAsync(userPermissions);
            }

            return true;
        }

        public async Task<bool> RemovePermissionFromUserAsync(int userId, int permissionId)
        {
            var userPermissions = await _userPermissionRepository.GetByUserIdAsync(userId);
            var up = userPermissions.FirstOrDefault(x => x.PermissionId == permissionId);
            
            if (up != null)
            {
                await _userPermissionRepository.DeleteAsync(up.Id);
            }

            return true;
        }

        public async Task<bool> RemoveAllDirectPermissionsFromUserAsync(int userId)
        {
            await _userPermissionRepository.DeleteByUserIdAsync(userId);
            return true;
        }

        #endregion
    }
}
