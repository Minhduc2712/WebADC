using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class CoverTypeRepository : ICoverTypeRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public CoverTypeRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<Cover_type?> GetByIdAsync(int id)
        {
            return await _context.CoverTypes
                .FirstOrDefaultAsync(c => c.Id == id && !c.Is_deleted);
        }

        public async Task<Cover_type?> GetByCodeAsync(string code)
        {
            return await _context.CoverTypes
                .FirstOrDefaultAsync(c => c.Cover_type_code == code && !c.Is_deleted);
        }

        public async Task<Cover_type?> GetByNameAsync(string name)
        {
            return await _context.CoverTypes
                .FirstOrDefaultAsync(c => c.Cover_type_name == name && !c.Is_deleted);
        }

        public async Task<IEnumerable<Cover_type>> GetAllAsync()
        {
            return await _context.CoverTypes
                .Where(c => !c.Is_deleted)
                .OrderBy(c => c.Cover_type_name)
                .ToListAsync();
        }

        public async Task<Cover_type> AddAsync(Cover_type coverType)
        {
            _context.CoverTypes.Add(coverType);
            await _context.SaveChangesAsync();
            return coverType;
        }

        public async Task UpdateAsync(Cover_type coverType)
        {
            _context.CoverTypes.Update(coverType);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var coverType = await _context.CoverTypes.FindAsync(id);
            if (coverType != null)
            {
                coverType.Is_deleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
