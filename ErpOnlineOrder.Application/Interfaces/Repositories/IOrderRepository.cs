using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task<Order?> GetByIdForStatusCheckAsync(int id);
        Task<Order?> GetByIdForCopyAsync(int id);
        IQueryable<Order?> GetByOrderIdAsync(int id);
        Task<IEnumerable<OrderDTO>> GetAllAsync(); 
        Task<int> CountAsync(Expression<Func<Order, bool>>? predicate = null);        
        Task<PagedResult<Order>> GetPagedOrdersAsync(OrderFilterRequest request, IEnumerable<int>? customerIds = null);
        Task<PagedResult<OrderDTO>> GetPagedOrdersDTOAsync(OrderFilterRequest request, IEnumerable<int>? customerIds = null);
        Task<IEnumerable<Order>> GetByCustomerIdsAsync(IEnumerable<int> customerIds);
        Task<IEnumerable<OrderDTO>> GetByCustomerIdsDTOAsync(IEnumerable<int> customerIds);

        Task<Order?> GetByCodeAsync(string code);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int id);
    }
}