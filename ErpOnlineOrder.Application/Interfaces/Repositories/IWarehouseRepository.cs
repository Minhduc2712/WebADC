using ErpOnlineOrder.Application.DTOs.WarehouseDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IWarehouseRepository
    {
        Task<Warehouse?> GetByIdAsync(int id);
        Task<Warehouse?> GetByCodeAsync(string code);
        Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
        Task<Warehouse?> GetByNameAsync(string name);
        Task<IEnumerable<Warehouse>> GetAllAsync();
        Task<bool> AnyAsync();
        Task<IEnumerable<Warehouse>> GetByProvinceIdAsync(int provinceId);
        Task<IEnumerable<WarehouseSelectDto>> GetForSelectAsync();
        Task<Warehouse> AddAsync(Warehouse warehouse);
        Task UpdateAsync(Warehouse warehouse);
        Task DeleteAsync(int id);
    }
}
