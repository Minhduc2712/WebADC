using System;
using System.Linq;
using System.Threading.Tasks;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Security;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserRegistrationService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> RegisterAsync(RegisterCustomerDto dto)
        {
            if (await _userRepository.ExistsByUsernameAsync(dto.Username))
                return false;

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = _passwordHasher.Hash(dto.Password),
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
