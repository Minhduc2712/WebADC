using System.Collections.Generic;
using System.Linq;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using ErpOnlineOrder.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public UserRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        private IQueryable<User> GetBaseQuery(bool includeDetails = false)
        {
            var query = _context.Users.AsNoTracking().Where(u => !u.Is_deleted);
            if (includeDetails)
            {
                query = query
                    .Include(u => u.User_roles)
                        .ThenInclude(ur => ur.Role)
                    .Include(u => u.Staff)
                    .Include(u => u.Customer);
            }
            return query;
        }

        public async Task<int> CountActiveStaffAsync()
        {
            return await _context.Users
                .CountAsync(u => u.Staff != null && !u.Is_deleted && u.Is_active);
        }

        public async Task<PagedResult<User>> GetPagedStaffAsync(StaffFilterRequest request)
        {
            var query = _context.Users.AsNoTracking()
                .Include(u => u.Staff)
                .Include(u => u.User_roles).ThenInclude(ur => ur.Role)
                .Where(u => u.Staff != null && !u.Is_deleted);

            if (request.RoleId.HasValue)
            {
                query = query.Where(u => u.User_roles.Any(ur => ur.Role_id == request.RoleId.Value));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.Is_active == request.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(u =>
                    (u.Username != null && u.Username.ToLower().Contains(search)) ||
                    (u.Email != null && u.Email.ToLower().Contains(search)) ||
                    (u.Staff != null && u.Staff.Full_name != null && u.Staff.Full_name.ToLower().Contains(search)) ||
                    (u.Staff != null && u.Staff.Staff_code != null && u.Staff.Staff_code.ToLower().Contains(search))
                );
            }

            query = query.OrderBy(u => u.Staff!.Full_name);
            return await query.ToPagedListAsync(request);
        }

        public async Task AddAsync(User user)
        {
            user.Created_at = DateTime.Now;
            user.Updated_at = DateTime.Now;
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id); if (user != null)
            {
                user.Is_deleted = true;
                user.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await GetBaseQuery(true).ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await GetBaseQuery()
                .Include(u => u.User_roles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.Role_Permissions)
                            .ThenInclude(rp => rp.Permission)
                .Include(u => u.Staff)
                .Include(u => u.Customer)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByIdBasicAsync(int id)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> FindByIdentifierAsync(string identifier)
        {
            return await GetBaseQuery()
                .Include(u => u.User_roles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.Role_Permissions)
                            .ThenInclude(rp => rp.Permission)
                .Include(u => u.Staff)
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Username == identifier || u.Email == identifier);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await GetBaseQuery(true)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByUsernameBasicAsync(string username)
        {
            return await GetBaseQuery()
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await GetBaseQuery()
                .Include(u => u.User_roles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByEmailBasicAsync(string email)
        {
            return await GetBaseQuery()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task UpdateAsync(User user)
        {
            user.Updated_at = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AssignRoleAsync(int userId, int roleId)
        {
            var exists = await _context.UserRoles
                .AsNoTracking()
                .AnyAsync(ur => ur.User_id == userId && ur.Role_id == roleId);

            if (exists) return true;

            var userRole = new User_role
            {
                User_id = userId,
                Role_id = roleId,
                Created_at = DateTime.Now,
                Updated_at = DateTime.Now,
                Is_deleted = false
            };

            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveRoleAsync(int userId, int roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.User_id == userId && ur.Role_id == roleId);

            if (userRole != null)
            {
                userRole.Is_deleted = true;
                userRole.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId)
        {
            return await GetBaseQuery(true)
                .Where(u => u.User_roles.Any(ur => ur.Role_id == roleId))
                .ToListAsync();
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username && !u.Is_deleted);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email && !u.Is_deleted);
        }

        public async Task<bool> ExistsByEmailAsync(string email, int excludeId)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email && u.Id != excludeId && !u.Is_deleted);
        }

        public async Task<User?> GetForUpdateAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Staff)
                .Include(u => u.User_roles)
                .FirstOrDefaultAsync(u => u.Id == id && !u.Is_deleted);
        }

        public async Task<User?> GetForLoginAsync(string identifier)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    (u.Username == identifier || u.Email == identifier)
                    && !u.Is_deleted);
        }
        
        public async Task<User?> GetForTokenValidationAsync(string username)
        {
            return await _context.Users
                .AsNoTracking()
                .Select(u => new User { 
                    Username = u.Username, 
                    Password = u.Password,
                })
                .FirstOrDefaultAsync(u => u.Username == username && !u.Is_deleted);
        }
    }
}
