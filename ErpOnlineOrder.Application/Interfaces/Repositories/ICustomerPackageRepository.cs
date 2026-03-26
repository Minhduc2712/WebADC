using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface ICustomerPackageRepository
    {
        Task<Customer_package?> GetByIdAsync(int id, bool includeDetails = false);
        Task<Customer_package?> GetByCustomerAndPackageAsync(int customerId, int packageId, bool includeDetails = false);
        Task<IEnumerable<Customer_package>> GetByCustomerIdAsync(int customerId, bool includeDetails = false);
        Task<IEnumerable<Customer_package>> GetByPackageIdAsync(int packageId, bool includeDetails = false);
        Task<IEnumerable<int>> GetProductIdsByCustomerIdAsync(int customerId);
        Task<Customer_package> AddAsync(Customer_package entity);
        Task UpdateAsync(Customer_package entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int customerId, int packageId);
    }
}