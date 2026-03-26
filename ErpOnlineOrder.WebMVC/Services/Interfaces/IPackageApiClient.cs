using ErpOnlineOrder.Application.DTOs.PackageDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IPackageApiClient
    {
        Task<IEnumerable<PackageDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PackageDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(PackageDto? Data, string? Error)> CreateAsync(CreatePackageDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdatePackageDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> AddProductAsync(int packageId, CreatePackageProductDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> RemoveProductAsync(int packageId, int productId, CancellationToken cancellationToken = default);
    }
}
