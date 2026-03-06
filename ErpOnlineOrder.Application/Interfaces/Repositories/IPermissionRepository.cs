using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IPermissionRepository
    {
        Task<Permission?> GetByIdAsync(int id);
        Task<Permission?> GetByCodeAsync(string permissionCode);
        Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
        Task<IEnumerable<Permission>> GetAllAsync();
        Task<IEnumerable<Permission>> GetByParentIdAsync(int? parentId);
        Task<IEnumerable<Permission>> GetSpecialPermissionsAsync();
        Task<Permission> AddAsync(Permission permission);
        Task UpdateAsync(Permission permission);
        Task DeleteAsync(int id);
    }
}
