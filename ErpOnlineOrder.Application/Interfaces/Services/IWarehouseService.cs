using ErpOnlineOrder.Application.DTOs.WarehouseDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IWarehouseService
    {
        Task<Warehouse?> GetByIdAsync(int id);
        Task<IEnumerable<Warehouse>> GetAllAsync();
        Task<IEnumerable<Warehouse>> GetByProvinceIdAsync(int provinceId);
        Task<Warehouse> CreateAsync(CreateWarehouseDto dto, int createdBy);
        Task<bool> UpdateAsync(UpdateWarehouseDto dto, int updatedBy);
        Task<bool> DeleteAsync(int id);
    }
}
