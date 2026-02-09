using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface ICoverTypeRepository
    {
        Task<Cover_type?> GetByIdAsync(int id);
        Task<Cover_type?> GetByCodeAsync(string code);
        Task<Cover_type?> GetByNameAsync(string name);
        Task<IEnumerable<Cover_type>> GetAllAsync();
        Task<Cover_type> AddAsync(Cover_type coverType);
        Task UpdateAsync(Cover_type coverType);
        Task DeleteAsync(int id);
    }
}
