using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IRolePermissionRepository
    {
        Task<Role_permission?> GetByIdAsync(int id);
        Task<IEnumerable<Role_permission>> GetByRoleIdAsync(int roleId);
        Task<IEnumerable<Role_permission>> GetByPermissionIdAsync(int permissionId);
        Task<Role_permission> AddAsync(Role_permission rolePermission);
        Task AddRangeAsync(IEnumerable<Role_permission> rolePermissions);
        Task DeleteAsync(int id);
        Task DeleteByRoleIdAsync(int roleId);
        Task<bool> ExistsAsync(int roleId, int permissionId);
    }
}
