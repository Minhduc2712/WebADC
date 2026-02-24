using ErpOnlineOrder.Application.DTOs.AuthDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public interface IAuthApiClient
    {
        Task<LoginResponseDto?> LoginAsync(LoginUserDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? ErrorMessage)> RegisterCustomerAsync(RegisterCustomerDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? ErrorMessage)> RegisterStaffAsync(RegisterStaffDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken cancellationToken = default);
    }
}
