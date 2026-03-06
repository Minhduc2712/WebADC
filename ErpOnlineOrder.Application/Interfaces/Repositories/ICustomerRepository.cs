using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetByUserIdAsync(int userId);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<int> CountAsync(Expression<Func<Customer, bool>>? predicate = null);        Task<PagedResult<Customer>> GetPagedCustomersAsync(CustomerFilterRequest request);
        Task<IEnumerable<CustomerSelectDto>> GetForSelectAsync();
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int id);
    }
}