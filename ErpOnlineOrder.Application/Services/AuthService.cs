using BCrypt.Net;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Security;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Enums;
using ErpOnlineOrder.Domain.Models;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        private readonly IRoleRepository _roleRepository;

        private readonly IPasswordHasher _passwordHasher;
        public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
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

        //public async Task<bool> LogoutAsync()
        //{
            
        //}
    }
}
