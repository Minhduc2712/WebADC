using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.DTOs.StockDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;

        public StockService(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<IEnumerable<Stock>> GetAllAsync(int? warehouseId = null, string? searchTerm = null)
        {
            return await _stockRepository.GetAllAsync(warehouseId, searchTerm);
        }

        public async Task<Stock?> GetByIdAsync(int id)
        {
            return await _stockRepository.GetByIdAsync(id);
        }

        public async Task<Stock> CreateAsync(CreateStockDto dto, int createdBy)
        {
            if (dto.Quantity < 0)
                throw new InvalidOperationException("Số lượng tồn không được âm");

            var existed = await _stockRepository.GetByWarehouseAndProductAsync(dto.Warehouse_id, dto.Product_id);
            if (existed != null)
            {
                throw new InvalidOperationException("Tồn kho cho kho và sản phẩm này đã tồn tại. Vui lòng dùng chức năng chỉnh sửa để cập nhật số lượng.");
            }

            var now = DateTime.UtcNow;
            var stock = new Stock
            {
                Warehouse_id = dto.Warehouse_id,
                Product_id = dto.Product_id,
                Quantity = dto.Quantity,
                Created_by = createdBy,
                Created_at = now,
                Updated_by = createdBy,
                Updated_at = now,
                Is_deleted = false
            };

            return await _stockRepository.AddAsync(stock);
        }

        public async Task<bool> UpdateQuantityAsync(int id, int quantity, int updatedBy)
        {
            if (quantity < 0)
                throw new InvalidOperationException("Số lượng tồn không được âm");

            var existing = await _stockRepository.GetByIdAsync(id);
            if (existing == null)
                return false;

            existing.Quantity = quantity;
            existing.Updated_by = updatedBy;
            existing.Updated_at = DateTime.UtcNow;

            await _stockRepository.UpdateAsync(existing);
            return true;
        }
    }
}
