using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface ICustomerManagementApiClient
    {
        Task<IEnumerable<Customer_management>> GetAllAsync(int? staffId, int? provinceId, CancellationToken cancellationToken = default);
        Task<Customer_management?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Customer_management>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default);
        Task<(Customer_management? Data, string? Error)> CreateAsync(Customer_management model, CancellationToken cancellationToken = default);
        Task<(Customer_management? Result, string? Error)> AssignStaffAsync(int staffId, int customerId, int provinceId, int? wardId = null, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, Customer_management model, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> IsAlreadyAssignedAsync(int staffId, int customerId, CancellationToken cancellationToken = default);
    }
}
