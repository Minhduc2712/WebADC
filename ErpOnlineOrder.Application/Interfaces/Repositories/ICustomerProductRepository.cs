using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface ICustomerProductRepository
    {
        Task<Customer_product?> GetByIdAsync(int id);
        Task<Customer_product?> GetByCustomerAndProductAsync(int customerId, int productId);
        Task<IEnumerable<Customer_product>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Customer_product>> GetByProductIdAsync(int productId);
        Task<IEnumerable<Customer_product>> GetAllAsync();
        Task<Customer_product> AddAsync(Customer_product customerProduct);
        Task AddRangeAsync(IEnumerable<Customer_product> customerProducts);
        Task UpdateAsync(Customer_product customerProduct);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int customerId, int productId);
    }
}
