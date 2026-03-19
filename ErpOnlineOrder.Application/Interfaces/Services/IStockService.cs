using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Application.DTOs.StockDTOs;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IStockService
    {
        Task<IEnumerable<Stock>> GetAllAsync(int? warehouseId = null, string? searchTerm = null);
        Task<Stock?> GetByIdAsync(int id);
        Task<Stock> CreateAsync(CreateStockDto dto, int createdBy);
        Task<bool> UpdateQuantityAsync(int id, int quantity, int updatedBy);
    }
}
