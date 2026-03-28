using ErpOnlineOrder.Application.DTOs.AdminDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;
using BCrypt.Net;

namespace ErpOnlineOrder.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IPermissionService _permissionService;

        public AdminService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            ICustomerRepository customerRepository,
            IOrderRepository orderRepository,
            IRolePermissionRepository rolePermissionRepository,
            IPermissionService permissionService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _permissionService = permissionService;
        }

        public async Task<IEnumerable<StaffAccountDto>> GetAllStaffAsync(int? currentUserId = null)
        {
            var staffUsers = await _userRepository.GetAllStaffAsync();
            var list = staffUsers.Select(u => EntityMappers.ToStaffAccountDto(u)).ToList();
            if (currentUserId.HasValue && currentUserId.Value > 0)
            {
                var userPermissions = (await _permissionService.GetUserPermissionsAsync(currentUserId.Value)).ToHashSet();
                foreach (var dto in list)
                    RecordPermissionEnricher.EnrichStaff(dto, userPermissions);
            }
            return list;
        }

        public async Task<PagedResult<StaffAccountDto>> GetStaffPagedAsync(StaffFilterRequest request, int? currentUserId = null)
        {
            var paged = await _userRepository.GetPagedStaffAsync(request);
            var list = paged.Items.Select(u => EntityMappers.ToStaffAccountDto(u)).ToList();
            if (currentUserId.HasValue && currentUserId.Value > 0)
            {
                var userPermissions = (await _permissionService.GetUserPermissionsAsync(currentUserId.Value)).ToHashSet();
                foreach (var dto in list)
                    RecordPermissionEnricher.EnrichStaff(dto, userPermissions);
            }
            return new PagedResult<StaffAccountDto>
            {
                Items = list,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            };
        }

        public async Task<StaffAccountDto?> GetStaffByIdAsync(int userId, int? currentUserId = null)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Staff == null)
                return null;
            var dto = EntityMappers.ToStaffAccountDto(user);
            if (currentUserId.HasValue && currentUserId.Value > 0)
                await RecordPermissionEnricher.EnrichStaffAsync(dto, currentUserId.Value, _permissionService);
            return dto;
        }

        public async Task<StaffAccountDto?> CreateStaffAccountAsync(CreateStaffAccountDto dto, int createdBy)
        {
            if (await _userRepository.ExistsByUsernameAsync(dto.Username))
            {
                throw new InvalidOperationException($"Tên đăng nhập '{dto.Username}' đã tồn tại.");
            }

            if (await _userRepository.ExistsByEmailAsync(dto.Email))
            {
                throw new InvalidOperationException($"Email '{dto.Email}' đã được sử dụng.");
            }

            var now = DateTime.UtcNow;
            
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Is_active = true,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = now,
                Updated_at = now,
                Is_deleted = false,
                Staff = new Staff
                {
                    Staff_code = $"STAFF-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                    Full_name = dto.Full_name,
                    Phone_number = dto.Phone_number,
                    Created_by = createdBy,
                    Updated_by = createdBy,
                    Created_at = now,
                    Updated_at = now,
                    Is_deleted = false
                }
            };

            if (dto.Role_ids != null && dto.Role_ids.Any())
            {
                user.User_roles = dto.Role_ids.Select(roleId => new User_role
                {
                    Role_id = roleId,
                    Created_at = now,
                    Updated_at = now,
                    Is_deleted = false
                }).ToList();
            }

            await _userRepository.AddAsync(user);

            var createdUser = await _userRepository.GetByUsernameAsync(dto.Username);
            return createdUser != null ? EntityMappers.ToStaffAccountDto(createdUser) : null;
        }

        public async Task<bool> UpdateStaffAccountAsync(UpdateStaffAccountDto dto, int updatedBy)
        {
            var user = await _userRepository.GetForUpdateAsync(dto.User_id);
            if (user == null || user.Staff == null)
            {
                return false;
            }

            var now = DateTime.UtcNow;

            var normalizedEmail = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            if (!string.IsNullOrEmpty(normalizedEmail) && !string.Equals(normalizedEmail, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _userRepository.ExistsByEmailAsync(normalizedEmail, dto.User_id))
                {
                    throw new InvalidOperationException($"Email '{normalizedEmail}' đã được sử dụng bởi tài khoản khác.");
                }
                user.Email = normalizedEmail;
            }

            if (dto.Is_active.HasValue)
            {
                user.Is_active = dto.Is_active.Value;
            }

            if (!string.IsNullOrEmpty(dto.Full_name))
            {
                user.Staff.Full_name = dto.Full_name;
            }

            if (dto.Phone_number != null)
            {
                user.Staff.Phone_number = dto.Phone_number;
            }

            user.Updated_by = updatedBy;
            user.Updated_at = now;
            user.Staff.Updated_by = updatedBy;
            user.Staff.Updated_at = now;

            if (dto.Role_ids != null)
            {
                user.User_roles.Clear();

                foreach (var roleId in dto.Role_ids)
                {
                    user.User_roles.Add(new User_role
                    {
                        Role_id = roleId,
                        User_id = user.Id,
                        Created_at = now,
                        Updated_at = now
                    });
                }
            }
            
            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<bool> DeleteStaffAccountAsync(int userId, int deletedBy)
        {
            var user = await _userRepository.GetByIdBasicAsync(userId);            if (user == null || user.Staff == null)
            {
                return false;
            }
            await _userRepository.DeleteAsync(userId);
            return true;
        }

        public async Task<bool> ToggleStaffStatusAsync(int userId, bool isActive, int updatedBy)
        {
            var user = await _userRepository.GetByIdBasicAsync(userId);
            if (user == null || user.Staff == null)
            {
                return false;
            }

            user.Is_active = isActive;
            user.Updated_by = updatedBy;
            user.Updated_at = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto, int updatedBy)
        {
            var user = await _userRepository.GetByIdBasicAsync(dto.User_id);
            if (user == null)
            {
                return false;
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.New_password);
            user.Updated_by = updatedBy;
            user.Updated_at = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<int> GetActiveStaffCountAsync()
        {
            return await _userRepository.CountActiveStaffAsync();
        }

        public async Task<int> GetCustomerCountAsync()
        {
            return await _customerRepository.CountAsync();
        }

        public async Task<int> GetOrderCountAsync()
        {
            return await _orderRepository.CountAsync();
        }
    }
}
