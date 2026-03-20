using ErpOnlineOrder.Application.DTOs.AuthDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IAuthApiClient
    {
        Task<LoginResponseDto?> LoginAsync(LoginUserDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? ErrorMessage)> RegisterCustomerAsync(RegisterCustomerDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? ErrorMessage)> FinalizeCustomerRegistrationAsync(FinalizeCustomerRegistrationDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? ErrorMessage)> RegisterStaffAsync(RegisterStaffDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken cancellationToken = default);
        Task<string?> GenerateTokenAsync(string username, CancellationToken cancellationToken = default);
        Task<LoginResponseDto?> AutoLoginAsync(string username, string token, CancellationToken cancellationToken = default);
        Task<bool> CheckEmailExistsAsync(string email, CancellationToken cancellationToken = default);
    }
}
