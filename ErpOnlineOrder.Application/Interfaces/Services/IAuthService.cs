using System.Threading.Tasks;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterByAdminAsync(RegisterStaffDto dto);
        Task<bool> RegisterByCustomerAsync(RegisterCustomerDto dto);
        Task<string?> LoginAsync(LoginUserDto dto);
        Task<LoginResponseDto?> GetLoginResponseAsync(LoginUserDto dto);
        Task<bool> ChangePasswordAsync(ChangePasswordDto dto);
        Task<CheckUserPermissionsResultDto?> CheckUserPermissionsAsync(int userId);
        Task<GrantAllPermissionsResultDto?> GrantAllPermissionsAsync(int userId);
        Task<SeedAdminResultDto> SeedAdminAsync(int createdBy);
        Task<CheckAdminResultDto?> CheckAdminAsync();
        Task<bool> DeleteUserAsync(int id);
        Task<DebugUserPermissionsResultDto?> DebugUserPermissionsAsync(int userId);
        Task<DebugRolePermissionsResultDto?> DebugRolePermissionsAsync(int roleId);
    }
}
