using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using System.Security.Cryptography;
using System.Text;

namespace ErpOnlineOrder.Application.Services
{
    public class RememberMeService : IRememberMeService
    {
        private const string Secret = "ErpSecretKey2024";
        private readonly IUserRepository _userRepository;

        public RememberMeService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public string GenerateToken(User user)
        {
            var data = $"{user.Username}:{user.Password}:{Secret}";
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }

        public async Task<bool> ValidateTokenAsync(string username, string token)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(token))
                return false;

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
                return false;

            var expectedToken = GenerateToken(user);
            return token == expectedToken;
        }
    }
}
