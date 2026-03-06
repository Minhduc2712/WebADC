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

        private IQueryable<Region> GetBaseQuery(bool includeDetails = false)
        {
            var query = _context.Regions.AsNoTracking();
            if (includeDetails)
            {
                query = query.Include(r => r.Provinces);
            }
            return query;
        }

        public async Task<Region?> GetByIdAsync(int id)
        {
            return await GetBaseQuery(true).FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Region?> GetByCodeAsync(string regionCode)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(r => r.Region_code == regionCode);
        }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        {
            var query = GetBaseQuery().Where(r => r.Region_code == code);
            if (excludeId.HasValue)
                query = query.Where(r => r.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Region?> GetByNameAsync(string regionName)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(r => r.Region_name == regionName);
        }

        public async Task<IEnumerable<Region>> GetAllAsync()
        {
            return await GetBaseQuery(true)
                .OrderBy(r => r.Region_name)
                .ToListAsync();
        }

        public async Task AddAsync(Region region)
        {
            region.Created_at = DateTime.Now;
            region.Updated_at = DateTime.Now;
            await _context.Regions.AddAsync(region);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Region region)
        {
            region.Updated_at = DateTime.Now;
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
