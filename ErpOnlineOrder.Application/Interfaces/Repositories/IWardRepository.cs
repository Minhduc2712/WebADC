using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IWardRepository
    {
        Task<Ward?> GetByIdAsync(int id);
        Task<Ward?> GetByCodeAsync(string code);
        Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
        Task<IEnumerable<Ward>> GetAllAsync();
        Task<IEnumerable<Ward>> GetByProvinceIdAsync(int provinceId);
        Task AddAsync(Ward ward);
        Task UpdateAsync(Ward ward);
        Task DeleteAsync(int id);
    }
}
