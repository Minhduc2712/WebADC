using ErpOnlineOrder.Application.DTOs.AuthDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    /// <summary>
    /// Client gọi WebAPI cho các thao tác Auth (Login, Register, ChangePassword).
    /// </summary>
    public interface IAuthApiClient
    {
        /// <summary>
        /// Đăng nhập. Trả về thông tin user + permissions nếu thành công; null nếu thất bại.
        /// </summary>
        Task<LoginResponseDto?> LoginAsync(LoginUserDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Đăng ký khách hàng.
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> RegisterCustomerAsync(RegisterCustomerDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Đăng ký nhân viên (admin).
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> RegisterStaffAsync(RegisterStaffDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Đổi mật khẩu.
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken cancellationToken = default);
    }
}
