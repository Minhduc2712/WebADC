using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebMVC.Services
{
    public interface ICustomerManagementApiClient
    {
        Task<IEnumerable<Customer_management>> GetAllAsync(int? staffId, int? provinceId, CancellationToken cancellationToken = default);
        Task<Customer_management?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Customer_management>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default);
        Task<Customer_management?> CreateAsync(Customer_management model, CancellationToken cancellationToken = default);
        Task<Customer_management?> AssignStaffAsync(int staffId, int customerId, int provinceId, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, Customer_management model, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> IsAlreadyAssignedAsync(int staffId, int customerId, CancellationToken cancellationToken = default);
    }
}
