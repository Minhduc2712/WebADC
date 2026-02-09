using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface ICustomerCategoryRepository
    {
        Task<Customer_category?> GetByIdAsync(int id);
        Task<Customer_category?> GetByCustomerAndCategoryAsync(int customerId, int categoryId);
        Task<IEnumerable<Customer_category>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Customer_category>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Customer_category>> GetAllAsync();
        Task<Customer_category> AddAsync(Customer_category customerCategory);
        Task AddRangeAsync(IEnumerable<Customer_category> customerCategories);
        Task UpdateAsync(Customer_category customerCategory);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int customerId, int categoryId);
    }
}
