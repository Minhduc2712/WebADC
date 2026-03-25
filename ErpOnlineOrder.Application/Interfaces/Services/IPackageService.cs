using ErpOnlineOrder.Application.DTOs.PackageDTOs;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IPackageService
    {
        Task<IEnumerable<PackageDto>> GetAllAsync();
        Task<PackageDto?> GetByIdAsync(int id);
        Task<IEnumerable<PackageDto>> GetByOrganizationAsync(int organizationId);
        Task<IEnumerable<PackageDto>> GetByRegionAsync(int regionId);
        Task<IEnumerable<PackageDto>> GetByProvinceAsync(int provinceId);
        Task<IEnumerable<PackageDto>> GetByWardAsync(int wardId);
        Task<PackageDto> CreateAsync(CreatePackageDto dto, int createdBy);
        Task<bool> UpdateAsync(int id, UpdatePackageDto dto, int updatedBy);
        Task<bool> DeleteAsync(int id);
        Task<bool> AddProductAsync(int packageId, CreatePackageProductDto dto, int createdBy);
        Task<bool> RemoveProductAsync(int packageId, int productId);
    }
}
