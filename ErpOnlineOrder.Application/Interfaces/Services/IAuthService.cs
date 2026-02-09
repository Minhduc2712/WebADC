using System.Threading.Tasks;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterByAdminAsync(RegisterStaffDto dto);
        Task<bool> RegisterByCustomerAsync(RegisterCustomerDto dto);
        Task<string?> LoginAsync(LoginUserDto dto); 
        Task<bool> ChangePasswordAsync(ChangePasswordDto dto);
        //Task<bool> LogoutAsync();
    }
}
