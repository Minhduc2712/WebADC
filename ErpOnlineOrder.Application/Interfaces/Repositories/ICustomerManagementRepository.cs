using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface ICustomerManagementRepository
    {
        Task<Customer_management?> GetByIdAsync(int id);
        Task<IEnumerable<Customer_management>> GetAllAsync();
        Task<IEnumerable<Customer_management>> GetByStaffAsync(int staffId);
        Task<IEnumerable<int>> GetCustomerIdsByStaffAsync(int staffId);
        Task<IEnumerable<Customer_management>> GetByCustomerAsync(int customerId);
        Task<IEnumerable<Customer_management>> GetByCustomerBasicAsync(int customerId);
        Task<Customer_management?> GetByStaffAndCustomerAsync(int staffId, int customerId);
        Task<bool> ExistsAsync(int staffId, int customerId, int? excludeId = null);
        Task<Customer_management?> FindDeletedAsync(int staffId, int customerId);
        Task<IEnumerable<Customer_management>> GetByProvinceAsync(int provinceId);
        Task<Customer_management?> FindAssignmentByProvinceAndWardAsync(int provinceId, int? wardId);
        Task AddAsync(Customer_management customerManagement);
        Task UpdateAsync(Customer_management customerManagement);
        Task DeleteAsync(int id);
    }
}