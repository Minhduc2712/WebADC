using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseService(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<Warehouse?> GetByIdAsync(int id)
        {
            return await _warehouseRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Warehouse>> GetAllAsync()
        {
            return await _warehouseRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Warehouse>> GetByProvinceIdAsync(int provinceId)
        {
            return await _warehouseRepository.GetByProvinceIdAsync(provinceId);
        }

        public async Task<Warehouse> CreateAsync(Warehouse warehouse)
        {
            // Kiểm tra trùng lặp Warehouse_code
            var existingByCode = await _warehouseRepository.GetByCodeAsync(warehouse.Warehouse_code);
            if (existingByCode != null)
            {
                throw new InvalidOperationException($"Kho hàng với mã '{warehouse.Warehouse_code}' đã tồn tại.");
            }

            // Kiểm tra trùng lặp Warehouse_name
            var existingByName = await _warehouseRepository.GetByNameAsync(warehouse.Warehouse_name);
            if (existingByName != null)
            {
                throw new InvalidOperationException($"Kho hàng với tên '{warehouse.Warehouse_name}' đã tồn tại.");
            }

            warehouse.Created_at = DateTime.Now;
            warehouse.Updated_at = DateTime.Now;
            return await _warehouseRepository.AddAsync(warehouse);
        }

        public async Task<bool> UpdateAsync(Warehouse warehouse)
        {
            var existing = await _warehouseRepository.GetByIdAsync(warehouse.Id);
            if (existing == null) return false;

            // Kiểm tra trùng lặp khi update (trừ bản ghi hiện tại)
            var existingByCode = await _warehouseRepository.GetByCodeAsync(warehouse.Warehouse_code);
            if (existingByCode != null && existingByCode.Id != warehouse.Id)
            {
                throw new InvalidOperationException($"Kho hàng với mã '{warehouse.Warehouse_code}' đã tồn tại.");
            }

            var existingByName = await _warehouseRepository.GetByNameAsync(warehouse.Warehouse_name);
            if (existingByName != null && existingByName.Id != warehouse.Id)
            {
                throw new InvalidOperationException($"Kho hàng với tên '{warehouse.Warehouse_name}' đã tồn tại.");
            }

            existing.Warehouse_code = warehouse.Warehouse_code;
            existing.Warehouse_name = warehouse.Warehouse_name;
            existing.Warehouse_address = warehouse.Warehouse_address;
            existing.Province_id = warehouse.Province_id;
            existing.Updated_by = warehouse.Updated_by;
            existing.Updated_at = DateTime.Now;
            await _warehouseRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _warehouseRepository.DeleteAsync(id);
            return true;
        }
    }
}
