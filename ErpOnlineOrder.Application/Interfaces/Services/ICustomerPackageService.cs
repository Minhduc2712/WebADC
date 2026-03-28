using ErpOnlineOrder.Application.DTOs.CustomerPackageDTOs;
namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface ICustomerPackageService
    {
        Task<bool> IsUserAllowedForCustomerAsync(int? userId, int customerId);  // ← ADD
        Task<PagedResult<CustomerPackageDto>> GetPagedAsync(int page, int pageSize, string? search = null, int? userId = null);
        Task<IEnumerable<CustomerPackageDto>> GetAllAsync();
        Task<CustomerPackageDto?> GetByIdAsync(int id);
        Task<CustomerPackageDto?> GetByCustomerAndPackageAsync(int customerId, int packageId);
        Task<IEnumerable<CustomerPackageDto>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<CustomerPackageDto>> GetByPackageIdAsync(int packageId);
        Task<IEnumerable<int>> GetProductIdsByCustomerIdAsync(int customerId);
        Task<CustomerPackageDto?> AssignPackageToCustomerAsync(CreateCustomerPackageDto dto, int createdBy);
        Task<bool> UnassignPackageFromCustomerAsync(int id, int deletedBy);
        Task<bool> UpdateAsync(int id, UpdateCustomerPackageDto dto, int updatedBy);
        Task<bool> ExistsAsync(int customerId, int packageId);
    }
}