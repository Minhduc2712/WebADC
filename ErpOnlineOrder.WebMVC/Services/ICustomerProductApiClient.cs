using ErpOnlineOrder.Application.DTOs.CustomerProductDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public interface ICustomerProductApiClient
    {
        Task<IEnumerable<CustomerProductDto>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<CustomerProductDto>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<CustomerProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CustomerProductDto?> CreateAsync(CreateCustomerProductDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> AssignProductsAsync(AssignProductsToCustomerDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCustomerProductDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
