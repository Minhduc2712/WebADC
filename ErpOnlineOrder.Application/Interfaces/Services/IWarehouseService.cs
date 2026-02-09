using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IWarehouseService
    {
        Task<Warehouse?> GetByIdAsync(int id);
        Task<IEnumerable<Warehouse>> GetAllAsync();
        Task<IEnumerable<Warehouse>> GetByProvinceIdAsync(int provinceId);
        Task<Warehouse> CreateAsync(Warehouse warehouse);
        Task<bool> UpdateAsync(Warehouse warehouse);
        Task<bool> DeleteAsync(int id);
    }
}
