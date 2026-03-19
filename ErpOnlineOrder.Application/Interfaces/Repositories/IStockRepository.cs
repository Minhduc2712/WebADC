using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IStockRepository
    {
        Task<IEnumerable<Stock>> GetAllAsync(int? warehouseId = null, string? searchTerm = null);
        Task<Stock?> GetByIdAsync(int id);
        Task<Stock?> GetByWarehouseAndProductAsync(int warehouseId, int productId);
        Task<Stock> AddAsync(Stock stock);
        Task UpdateAsync(Stock stock);
    }
}
