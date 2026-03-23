using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class WardRepository : IWardRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public WardRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        private IQueryable<Ward> GetBaseQuery(bool includeDetails = false)
        {
            var query = _context.Wards.AsNoTracking();
            if (includeDetails)
            {
                query = query.Include(w => w.Province);
            }
            return query;
        }

        public async Task<Ward?> GetByIdAsync(int id)
        {
            return await GetBaseQuery(true).FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Ward?> GetByCodeAsync(string wardCode)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(w => w.Ward_code == wardCode);
        }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        {
            var query = GetBaseQuery().Where(w => w.Ward_code == code);
            if (excludeId.HasValue)
                query = query.Where(w => w.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Ward>> GetAllAsync()
        {
            return await GetBaseQuery(true)
                .OrderBy(w => w.Ward_name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ward>> GetByProvinceIdAsync(int provinceId)
        {
            return await GetBaseQuery(true)
                .Where(w => w.Province_id == provinceId)
                .OrderBy(w => w.Ward_name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ward>> GetManyByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            if (!idList.Any()) return Array.Empty<Ward>();
            return await GetBaseQuery(false)
                .Where(w => idList.Contains(w.Id))
                .ToListAsync();
        }

        public async Task AddAsync(Ward ward)
        {
            ward.Created_at = DateTime.Now;
            ward.Updated_at = DateTime.Now;
            await _context.Wards.AddAsync(ward);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Ward ward)
        {
            ward.Updated_at = DateTime.Now;
            _context.Wards.Update(ward);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var ward = await _context.Wards.FindAsync(id);
            if (ward != null)
            {
                ward.Is_deleted = true;
                ward.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
