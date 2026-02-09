using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
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

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.Is_deleted = true;
                user.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.User_roles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Staff)
                .Include(u => u.Customer)
                .Where(u => !u.Is_deleted)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.User_roles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.Role_Permissions)
                            .ThenInclude(rp => rp.Permission)
                // .ThenInclude(p => p.Module)
                // .ThenInclude(p => p.Action)
                .Include(u => u.Staff)
                .Include(u => u.Customer)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == id && !u.Is_deleted);
        }

        public async Task<User?> FindByIdentifierAsync(string identifier)
        {
            return await _context.Users
                .Include(u => u.User_roles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.Role_Permissions)
                            .ThenInclude(rp => rp.Permission)
                .Include(u => u.Staff)
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => (u.Username == identifier || u.Email == identifier) && !u.Is_deleted);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.User_roles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Staff)
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Username == username && !u.Is_deleted);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.User_roles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email && !u.Is_deleted);
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
                .AnyAsync(ur => ur.User_id == userId && ur.Role_id == roleId && !ur.Is_deleted);

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
                .FirstOrDefaultAsync(ur => ur.User_id == userId && ur.Role_id == roleId && !ur.Is_deleted);

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
            return await _context.Users
                .Include(u => u.User_roles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Staff)
                .Include(u => u.Customer)
                .Where(u => !u.Is_deleted && u.User_roles.Any(ur => ur.Role_id == roleId && !ur.Is_deleted))
                .ToListAsync();
        }
    }
}
