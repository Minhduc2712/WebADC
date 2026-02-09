using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class RolePermissionRepository : IRolePermissionRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public RolePermissionRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<Role_permission?> GetByIdAsync(int id)
        {
            return await _context.RolePermissions
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                // .ThenInclude(p => p.Module)
                // .ThenInclude(p => p.Action)
                .FirstOrDefaultAsync(rp => rp.Id == id && !rp.Is_deleted);
        }

        public async Task<IEnumerable<Role_permission>> GetByRoleIdAsync(int roleId)
        {
            return await _context.RolePermissions
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                // .ThenInclude(p => p.Module)
                // .ThenInclude(p => p.Action)
                .Where(rp => rp.RoleId == roleId && !rp.Is_deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Role_permission>> GetByPermissionIdAsync(int permissionId)
        {
            return await _context.RolePermissions
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Where(rp => rp.PermissionId == permissionId && !rp.Is_deleted)
                .ToListAsync();
        }

        public async Task<Role_permission> AddAsync(Role_permission rolePermission)
        {
            rolePermission.Created_at = DateTime.Now;
            rolePermission.Updated_at = DateTime.Now;
            await _context.RolePermissions.AddAsync(rolePermission);
            await _context.SaveChangesAsync();
            return rolePermission;
        }

        public async Task AddRangeAsync(IEnumerable<Role_permission> rolePermissions)
        {
            var now = DateTime.Now;
            foreach (var rp in rolePermissions)
            {
                rp.Created_at = now;
                rp.Updated_at = now;
            }
            await _context.RolePermissions.AddRangeAsync(rolePermissions);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var rolePermission = await _context.RolePermissions.FindAsync(id);
            if (rolePermission != null)
            {
                rolePermission.Is_deleted = true;
                rolePermission.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByRoleIdAsync(int roleId)
        {
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && !rp.Is_deleted)
                .ToListAsync();

            var now = DateTime.Now;
            foreach (var rp in rolePermissions)
            {
                rp.Is_deleted = true;
                rp.Updated_at = now;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int roleId, int permissionId)
        {
            return await _context.RolePermissions
                .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.Is_deleted);
        }
    }
}
