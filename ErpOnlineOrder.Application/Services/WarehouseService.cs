using ErpOnlineOrder.Application.DTOs.WarehouseDTOs;
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

        public async Task<Warehouse> CreateAsync(CreateWarehouseDto dto, int createdBy)
        {
            var existingByCode = await _warehouseRepository.GetByCodeAsync(dto.Warehouse_code);
            if (existingByCode != null)
                throw new InvalidOperationException($"Kho hàng với mã '{dto.Warehouse_code}' đã tồn tại.");

            var existingByName = await _warehouseRepository.GetByNameAsync(dto.Warehouse_name);
            if (existingByName != null)
                throw new InvalidOperationException($"Kho hàng với tên '{dto.Warehouse_name}' đã tồn tại.");

            var warehouse = new Warehouse
            {
                Warehouse_code = dto.Warehouse_code,
                Warehouse_name = dto.Warehouse_name,
                Warehouse_address = dto.Warehouse_address,
                Province_id = dto.Province_id,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow
            };
            await _warehouseRepository.AddAsync(warehouse);
            return warehouse;
        }

        public async Task<bool> UpdateAsync(UpdateWarehouseDto dto, int updatedBy)
        {
            var existing = await _warehouseRepository.GetByIdAsync(dto.Id);
            if (existing == null) return false;

            var existingByCode = await _warehouseRepository.GetByCodeAsync(dto.Warehouse_code);
            if (existingByCode != null && existingByCode.Id != dto.Id)
                throw new InvalidOperationException($"Kho hàng với mã '{dto.Warehouse_code}' đã tồn tại.");

            var existingByName = await _warehouseRepository.GetByNameAsync(dto.Warehouse_name);
            if (existingByName != null && existingByName.Id != dto.Id)
                throw new InvalidOperationException($"Kho hàng với tên '{dto.Warehouse_name}' đã tồn tại.");

            existing.Warehouse_code = dto.Warehouse_code;
            existing.Warehouse_name = dto.Warehouse_name;
            existing.Warehouse_address = dto.Warehouse_address;
            existing.Province_id = dto.Province_id;
            existing.Updated_by = updatedBy;
            existing.Updated_at = DateTime.UtcNow;
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
