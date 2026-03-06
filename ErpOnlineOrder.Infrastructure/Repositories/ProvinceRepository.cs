using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class ProvinceRepository : IProvinceRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public ProvinceRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        private IQueryable<Province> GetBaseQuery(bool includeDetails = false)
        {
            var query = _context.Provinces.AsNoTracking();
            if (includeDetails)
            {
                query = query.Include(p => p.Region);
            }
            return query;
        }

        public async Task<Province?> GetByIdAsync(int id)
        {
            return await GetBaseQuery(true).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Province?> GetByCodeAsync(string provinceCode)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(p => p.Province_code == provinceCode);
        }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        {
            var query = GetBaseQuery().Where(p => p.Province_code == code);
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Province?> GetByNameAsync(string provinceName)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(p => p.Province_name == provinceName);
        }

        public async Task<IEnumerable<Province>> GetAllAsync()
        {
            return await GetBaseQuery(true)
                .OrderBy(p => p.Province_name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Province>> GetByRegionIdAsync(int regionId)
        {
            return await GetBaseQuery(true)
                .Where(p => p.Region_id == regionId)
                .OrderBy(p => p.Province_name)
                .ToListAsync();
        }

        public async Task AddAsync(Province province)
        {
            province.Created_at = DateTime.Now;
            province.Updated_at = DateTime.Now;
            await _context.Provinces.AddAsync(province);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Province province)
        {
            province.Updated_at = DateTime.Now;
            _context.Provinces.Update(province);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var province = await _context.Provinces.FindAsync(id);
            if (province != null)
            {
                province.Is_deleted = true;
                province.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
