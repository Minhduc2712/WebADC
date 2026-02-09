using ErpOnlineOrder.Application.DTOs.AdminDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
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

        public AdminService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            ICustomerRepository customerRepository,
            IOrderRepository orderRepository,
            IRolePermissionRepository rolePermissionRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _rolePermissionRepository = rolePermissionRepository;
        }

        #region Qu?n lý tài kho?n nhân viên

        public async Task<IEnumerable<StaffAccountDto>> GetAllStaffAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var staffUsers = users.Where(u => u.Staff != null && !u.Is_deleted);
            
            return staffUsers.Select(u => MapToStaffAccountDto(u));
        }

        public async Task<StaffAccountDto?> GetStaffByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Staff == null)
            {
                return null;
            }
            
            return MapToStaffAccountDto(user);
        }

        public async Task<StaffAccountDto?> CreateStaffAccountAsync(CreateStaffAccountDto dto, int createdBy)
        {
            // Ki?m tra username ?? t?n t?i
            var existingByUsername = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingByUsername != null)
            {
                throw new InvalidOperationException($"T?n ??ng nh?p '{dto.Username}' ?? t?n t?i trong h? th?ng.");
            }

            // Ki?m tra email ?? t?n t?i
            var existingByEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingByEmail != null)
            {
                throw new InvalidOperationException($"Email '{dto.Email}' ?? ???c s? d?ng b?i t?i kho?n kh?c.");
            }

            var now = DateTime.UtcNow;
            
            // T?o user m?i
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

            // G?n roles n?u c?
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

            // L?y l?i user ?? tr? v?
            var createdUser = await _userRepository.GetByUsernameAsync(dto.Username);
            return createdUser != null ? MapToStaffAccountDto(createdUser) : null;
        }

        public async Task<bool> UpdateStaffAccountAsync(UpdateStaffAccountDto dto, int updatedBy)
        {
            var user = await _userRepository.GetByIdAsync(dto.User_id);
            if (user == null || user.Staff == null)
            {
                return false;
            }

            var now = DateTime.UtcNow;

            // C?p nh?t email n?u c?
            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                var existingByEmail = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingByEmail != null && existingByEmail.Id != dto.User_id)
                {
                    throw new InvalidOperationException($"Email '{dto.Email}' ?? ???c s? d?ng b?i t?i kho?n kh?c.");
                }
                user.Email = dto.Email;
            }

            // C?p nh?t tr?ng th?i n?u c?
            if (dto.Is_active.HasValue)
            {
                user.Is_active = dto.Is_active.Value;
            }

            // C?p nh?t th?ng tin staff
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

            await _userRepository.UpdateAsync(user);

            // C?p nh?t roles n?u c?
            if (dto.Role_ids != null)
            {
                // X?a roles c?
                foreach (var userRole in user.User_roles.ToList())
                {
                    await _userRepository.RemoveRoleAsync(dto.User_id, userRole.Role_id);
                }

                // Th?m roles m?i
                foreach (var roleId in dto.Role_ids)
                {
                    await _userRepository.AssignRoleAsync(dto.User_id, roleId);
                }
            }

            return true;
        }

        public async Task<bool> DeleteStaffAccountAsync(int userId, int deletedBy)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Staff == null)
            {
                return false;
            }

            await _userRepository.DeleteAsync(userId);
            return true;
        }

        public async Task<bool> ToggleStaffStatusAsync(int userId, bool isActive, int updatedBy)
        {
            var user = await _userRepository.GetByIdAsync(userId);
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
            var user = await _userRepository.GetByIdAsync(dto.User_id);
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

        #endregion

        #region Th?ng k?

        public async Task<int> GetActiveStaffCountAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Count(u => u.Staff != null && !u.Is_deleted && u.Is_active);
        }

        public async Task<int> GetCustomerCountAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Count();
        }

        public async Task<int> GetOrderCountAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Count();
        }

        #endregion

        #region Private Methods

        private static StaffAccountDto MapToStaffAccountDto(User user)
        {
            return new StaffAccountDto
            {
                User_id = user.Id,
                Staff_id = user.Staff?.Id ?? 0,
                Staff_code = user.Staff?.Staff_code ?? string.Empty,
                Username = user.Username,
                Email = user.Email,
                Full_name = user.Staff?.Full_name ?? string.Empty,
                Phone_number = user.Staff?.Phone_number,
                Is_active = user.Is_active,
                Roles = user.User_roles
                    .Where(ur => !ur.Is_deleted && !ur.Role.Is_deleted)
                    .Select(ur => ur.Role.Role_name)
                    .ToList(),
                Created_at = user.Created_at
            };
        }

        #endregion
    }
}
