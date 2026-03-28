using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<int> CountActiveStaffAsync();
        Task<User?> GetByIdBasicAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<IEnumerable<User>> GetAllStaffAsync();
        Task<PagedResult<User>> GetPagedStaffAsync(StaffFilterRequest request);
        Task<User?> FindByIdentifierAsync(string identifier);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByUsernameBasicAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailBasicAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<bool> AssignRoleAsync(int userId, int roleId);
        Task<bool> RemoveRoleAsync(int userId, int roleId);
        Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email, int excludeId);
        Task<User?> GetForUpdateAsync(int id);
        Task<User?> GetForLoginAsync(string identifier);
        Task<User?> GetForTokenValidationAsync(string username);
    }
}
