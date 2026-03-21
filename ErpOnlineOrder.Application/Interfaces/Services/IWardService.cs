using ErpOnlineOrder.Application.DTOs.WardDTOs;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IWardService
    {
        Task<WardDTO?> GetByIdAsync(int id);
        Task<IEnumerable<WardDTO>> GetAllAsync();
        Task<IEnumerable<WardDTO>> GetByProvinceIdAsync(int provinceId);
        Task<WardDTO?> CreateWardAsync(CreateWardDto dto, int createdBy);
        Task<bool> UpdateWardAsync(int id, UpdateWardDto dto, int updatedBy);
        Task<bool> DeleteWardAsync(int id);
    }
}
