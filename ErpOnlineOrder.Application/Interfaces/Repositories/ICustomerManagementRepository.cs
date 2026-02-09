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
        Task<IEnumerable<Customer_management>> GetByCustomerAsync(int customerId);
        Task<Customer_management?> GetByStaffAndCustomerAsync(int staffId, int customerId);
        Task<IEnumerable<Customer_management>> GetByProvinceAsync(int provinceId);
        Task AddAsync(Customer_management customerManagement);
        Task UpdateAsync(Customer_management customerManagement);
        Task DeleteAsync(int id);
    }
}