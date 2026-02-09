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

        public async Task<Province?> GetByIdAsync(int id)
        {
            return await _context.Provinces
                .Include(p => p.Region)
                .FirstOrDefaultAsync(p => p.Id == id && !p.Is_deleted);
        }

        public async Task<Province?> GetByCodeAsync(string provinceCode)
        {
            return await _context.Provinces
                .FirstOrDefaultAsync(p => p.Province_code == provinceCode && !p.Is_deleted);
        }

        public async Task<Province?> GetByNameAsync(string provinceName)
        {
            return await _context.Provinces
                .FirstOrDefaultAsync(p => p.Province_name == provinceName && !p.Is_deleted);
        }

        public async Task<IEnumerable<Province>> GetAllAsync()
        {
            return await _context.Provinces
                .Include(p => p.Region)
                .Where(p => !p.Is_deleted)
                .OrderBy(p => p.Province_name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Province>> GetByRegionIdAsync(int regionId)
        {
            return await _context.Provinces
                .Include(p => p.Region)
                .Where(p => p.Region_id == regionId && !p.Is_deleted)
                .OrderBy(p => p.Province_name)
                .ToListAsync();
        }

        public async Task AddAsync(Province province)
        {
            await _context.Provinces.AddAsync(province);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Province province)
        {
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
