using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IUserPermissionRepository
    {
        Task<User_permission?> GetByIdAsync(int id);
        Task<IEnumerable<User_permission>> GetByUserIdAsync(int userId);
        Task<IEnumerable<User_permission>> GetByPermissionIdAsync(int permissionId);
        Task<User_permission> AddAsync(User_permission userPermission);
        Task AddRangeAsync(IEnumerable<User_permission> userPermissions);
        Task DeleteAsync(int id);
        Task DeleteByUserIdAsync(int userId);
        Task<bool> ExistsAsync(int userId, int permissionId);
    }
}
