using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class UserPermissionRepository : IUserPermissionRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public UserPermissionRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<User_permission?> GetByIdAsync(int id)
        {
            return await _context.UserPermissions
                .Include(up => up.User)
                .Include(up => up.Permission)
                // .ThenInclude(p => p.Module)
                // .ThenInclude(p => p.Action)
                .FirstOrDefaultAsync(up => up.Id == id && !up.IsDeleted);
        }

        public async Task<IEnumerable<User_permission>> GetByUserIdAsync(int userId)
        {
            return await _context.UserPermissions
                .Include(up => up.Permission)
                // .ThenInclude(p => p.Module)
                // .ThenInclude(p => p.Action)
                .Where(up => up.UserId == userId && !up.IsDeleted)
                .Where(up => up.ExpiresAt == null || up.ExpiresAt > DateTime.Now) // Chi lay quyen chua het han
                .ToListAsync();
        }

        public async Task<IEnumerable<User_permission>> GetByPermissionIdAsync(int permissionId)
        {
            return await _context.UserPermissions
                .Include(up => up.User)
                .Where(up => up.PermissionId == permissionId && !up.IsDeleted)
                .ToListAsync();
        }

        public async Task<User_permission> AddAsync(User_permission userPermission)
        {
            userPermission.GrantedAt = DateTime.Now;
            await _context.UserPermissions.AddAsync(userPermission);
            await _context.SaveChangesAsync();
            return userPermission;
        }

        public async Task AddRangeAsync(IEnumerable<User_permission> userPermissions)
        {
            var now = DateTime.Now;
            foreach (var up in userPermissions)
            {
                up.GrantedAt = now;
            }
            await _context.UserPermissions.AddRangeAsync(userPermissions);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var userPermission = await _context.UserPermissions.FindAsync(id);
            if (userPermission != null)
            {
                userPermission.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByUserIdAsync(int userId)
        {
            var userPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)  // Xoa tat ca, ke ca IsDeleted = true
                .ToListAsync();

            // Xoa thuc su de tranh loi unique constraint
            _context.UserPermissions.RemoveRange(userPermissions);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int userId, int permissionId)
        {
            return await _context.UserPermissions
                .AnyAsync(up => up.UserId == userId && up.PermissionId == permissionId && !up.IsDeleted);
        }
    }
}
