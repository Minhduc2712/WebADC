using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface ICoverTypeService
    {
        Task<Cover_type?> GetByIdAsync(int id);
        Task<IEnumerable<Cover_type>> GetAllAsync();
        Task<Cover_type> CreateAsync(Cover_type coverType);
        Task<bool> UpdateAsync(Cover_type coverType);
        Task<bool> DeleteAsync(int id);
    }
}
