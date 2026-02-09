using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Services;

/// <summary>
/// Dịch vụ tạo và xác thực token "Ghi nhớ đăng nhập" (Remember Me).
/// </summary>
public interface IRememberMeService
{
    /// <summary>
    /// Sinh token từ thông tin user (lưu vào cookie).
    /// </summary>
    string GenerateToken(User user);

    /// <summary>
    /// Kiểm tra token từ cookie có khớp với user hiện tại không.
    /// </summary>
    Task<bool> ValidateTokenAsync(string username, string token);
}
