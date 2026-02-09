using ErpOnlineOrder.Application.DTOs.AdminDTOs;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IAdminService
    {
        #region Qu?n lý tài kho?n nhân viên
        Task<IEnumerable<StaffAccountDto>> GetAllStaffAsync();
        Task<StaffAccountDto?> GetStaffByIdAsync(int userId);
        Task<StaffAccountDto?> CreateStaffAccountAsync(CreateStaffAccountDto dto, int createdBy);
        Task<bool> UpdateStaffAccountAsync(UpdateStaffAccountDto dto, int updatedBy);
        Task<bool> DeleteStaffAccountAsync(int userId, int deletedBy);
        Task<bool> ToggleStaffStatusAsync(int userId, bool isActive, int updatedBy);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto, int updatedBy);
        
        #endregion
        
        #region Th?ng kê
        Task<int> GetActiveStaffCountAsync();
        Task<int> GetCustomerCountAsync();
        Task<int> GetOrderCountAsync();
        
        #endregion
    }
}
