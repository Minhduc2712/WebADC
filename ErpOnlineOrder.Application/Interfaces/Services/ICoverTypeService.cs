using ErpOnlineOrder.Application.DTOs.CoverTypeDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface ICoverTypeService
    {
        Task<Cover_type?> GetByIdAsync(int id);
        Task<IEnumerable<Cover_type>> GetAllAsync();
        Task<Cover_type> CreateAsync(CreateCoverTypeDto dto, int createdBy);
        Task<bool> UpdateAsync(int id, UpdateCoverTypeDto dto, int updatedBy);
        Task<bool> DeleteAsync(int id);
    }
}
