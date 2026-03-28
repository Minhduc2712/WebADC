using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public StaffRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        private IQueryable<Staff> GetBaseQuery(bool includeDetails = false)
        {
            var query = _context.Staffs.AsNoTracking();
            if (includeDetails)
            {
                query = query.Include(s => s.User);
            }
            return query;
        }

        public async Task<Staff?> GetByIdAsync(int id)
        {
            return await GetBaseQuery(true).FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Staff?> GetByUserIdAsync(int userId)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(s => s.User_id == userId);
        }

        public async Task<IEnumerable<Staff>> GetAllAsync()
        {
            return await GetBaseQuery(true)
                .OrderBy(s => s.Full_name)
                .ToListAsync();
        }

        public async Task<Staff?> GetFirstAsync()
        {
            return await GetBaseQuery()
                .OrderBy(s => s.Id)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(Staff staff)
        {
            staff.Created_at = DateTime.Now;
            staff.Updated_at = DateTime.Now;
            await _context.Staffs.AddAsync(staff);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Staff staff)
        {
            staff.Updated_at = DateTime.Now;
            _context.Staffs.Update(staff);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            if (staff != null)
            {
                staff.Is_deleted = true;
                staff.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
