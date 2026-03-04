using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public PermissionRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<Permission?> GetByIdAsync(int id)
        {
            return await _context.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Permission?> GetByCodeAsync(string permissionCode)
        {
            return await _context.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Permission_code == permissionCode);
        }

        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            return await _context.Permissions
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetByParentIdAsync(int? parentId)
        {
            return await _context.Permissions
                .AsNoTracking()
                .Where(p => p.Parent_id == parentId)
                .OrderBy(p => p.Permission_code)
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetSpecialPermissionsAsync()
        {
            return await _context.Permissions
                .AsNoTracking()
                .Where(p => p.Is_special)
                .OrderBy(p => p.Permission_code)
                .ToListAsync();
        }

        public async Task<Permission> AddAsync(Permission permission)
        {
            permission.Created_at = DateTime.Now;
            permission.Updated_at = DateTime.Now;
            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();
            return permission;
        }

        public async Task UpdateAsync(Permission permission)
        {
            permission.Updated_at = DateTime.Now;
            _context.Permissions.Update(permission);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission != null)
            {
                permission.Is_deleted = true;
                permission.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
