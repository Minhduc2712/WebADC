using ErpOnlineOrder.Application.DTOs.AdminDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public interface IAdminApiClient
    {
        Task<IEnumerable<StaffAccountDto>> GetAllStaffAsync(CancellationToken cancellationToken = default);
        Task<StaffAccountDto?> GetStaffByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<StaffAccountDto?> CreateStaffAsync(CreateStaffAccountDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateStaffAsync(int userId, UpdateStaffAccountDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteStaffAsync(int userId, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> ToggleStaffStatusAsync(int userId, bool isActive, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
    }
}
