using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IWarehouseRepository
    {
        Task<Warehouse?> GetByIdAsync(int id);
        Task<Warehouse?> GetByCodeAsync(string code);
        Task<Warehouse?> GetByNameAsync(string name);
        Task<IEnumerable<Warehouse>> GetAllAsync();
        Task<IEnumerable<Warehouse>> GetByProvinceIdAsync(int provinceId);
        Task<Warehouse> AddAsync(Warehouse warehouse);
        Task UpdateAsync(Warehouse warehouse);
        Task DeleteAsync(int id);
    }
}
