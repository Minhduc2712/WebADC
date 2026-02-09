using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class RegionRepository : IRegionRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public RegionRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<Region?> GetByIdAsync(int id)
        {
            return await _context.Regions
                .Include(r => r.Provinces)
                .FirstOrDefaultAsync(r => r.Id == id && !r.Is_deleted);
        }

        public async Task<Region?> GetByCodeAsync(string regionCode)
        {
            return await _context.Regions
                .FirstOrDefaultAsync(r => r.Region_code == regionCode && !r.Is_deleted);
        }

        public async Task<Region?> GetByNameAsync(string regionName)
        {
            return await _context.Regions
                .FirstOrDefaultAsync(r => r.Region_name == regionName && !r.Is_deleted);
        }

        public async Task<IEnumerable<Region>> GetAllAsync()
        {
            return await _context.Regions
                .Include(r => r.Provinces)
                .Where(r => !r.Is_deleted)
                .OrderBy(r => r.Region_name)
                .ToListAsync();
        }

        public async Task AddAsync(Region region)
        {
            await _context.Regions.AddAsync(region);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Region region)
        {
            _context.Regions.Update(region);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var region = await _context.Regions.FindAsync(id);
            if (region != null)
            {
                region.Is_deleted = true;
                region.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
