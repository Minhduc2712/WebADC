using System;
using System.Linq;
using System.Threading.Tasks;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly IUserRepository _userRepository;

        public UserRegistrationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> RegisterAsync(RegisterCustomerDto dto)
        {
            var users = await _userRepository.GetAllAsync();
            if (users.Any(u => u.Username == dto.Username))
                return false;

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = dto.Password, 
                Is_active = true,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false
            };

            await _userRepository.AddAsync(user);
            return true;
        }
    }
}
