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

        public async Task<Staff?> GetByIdAsync(int id)
        {
            return await _context.Staffs
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id && !s.Is_deleted);
        }

        public async Task<IEnumerable<Staff>> GetAllAsync()
        {
            return await _context.Staffs
                .Where(s => !s.Is_deleted)
                .Include(s => s.User)
                .OrderBy(s => s.Full_name)
                .ToListAsync();
        }

        public async Task AddAsync(Staff staff)
        {
            await _context.Staffs.AddAsync(staff);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Staff staff)
        {
            _context.Staffs.Update(staff);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            if (staff != null)
            {
                staff.Is_deleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
