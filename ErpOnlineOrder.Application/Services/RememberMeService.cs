using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

public class RememberMeService : IRememberMeService
{
    private readonly string _secret;
    private readonly IUserRepository _userRepository;

    public RememberMeService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _secret = configuration["AuthSettings:RememberMeSecret"] ?? "FallbackSecret123!";
    }

    public string GenerateToken(User user)
    {
        // Thêm một giá trị salt hoặc thời gian nếu cần thiết
        var data = $"{user.Username}:{user.Password}:{_secret}";
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }

    public async Task<bool> ValidateTokenAsync(string username, string token)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(token))
            return false;

        // Tối ưu: Chỉ lấy Username/Password, không lấy Staff/Customer/Roles
        var user = await _userRepository.GetForTokenValidationAsync(username);
        
        if (user == null) return false;

        var expectedToken = GenerateToken(user);
        
        // Sử dụng CryptographicOperations.FixedTimeEquals để chống Timing Attack
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(token), 
            Encoding.UTF8.GetBytes(expectedToken));
    }
}