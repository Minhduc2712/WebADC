using ErpOnlineOrder.Application.DTOs.CustomerPackageDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface ICustomerPackageApiClient
    {
        Task<IEnumerable<CustomerPackageDto>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<CustomerPackageDto>> GetByPackageIdAsync(int packageId, CancellationToken cancellationToken = default);
        Task<CustomerPackageDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(CustomerPackageDto? Data, string? Error)> AssignPackageAsync(CreateCustomerPackageDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCustomerPackageDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UnassignPackageAsync(int id, CancellationToken cancellationToken = default);
    }
}
