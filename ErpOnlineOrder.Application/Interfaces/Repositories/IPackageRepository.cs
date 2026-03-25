using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IPackageRepository
    {
        Task<Package?> GetByIdAsync(int id);
        Task<IEnumerable<Package>> GetAllAsync();
        Task<IEnumerable<Package>> GetByOrganizationAsync(int organizationId);
        Task<IEnumerable<Package>> GetByRegionAsync(int regionId);
        Task<IEnumerable<Package>> GetByProvinceAsync(int provinceId);
        Task<IEnumerable<Package>> GetByWardAsync(int wardId);
        Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
        Task AddAsync(Package package);
        Task UpdateAsync(Package package);
        Task DeleteAsync(int id);
        Task<Package_product?> GetPackageProductAsync(int packageId, int productId);
        Task AddPackageProductAsync(Package_product packageProduct);
        Task RemovePackageProductAsync(Package_product packageProduct);
    }
}
