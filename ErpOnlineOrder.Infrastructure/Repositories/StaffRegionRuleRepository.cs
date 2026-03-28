using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class StaffRegionRuleRepository : IStaffRegionRuleRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public StaffRegionRuleRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        private IQueryable<Staff_region_rule> GetBaseQuery(bool includeDetails = false)
        {
            var query = _context.StaffRegionRules.AsNoTracking();
            if (includeDetails)
            {
                query = query
                    .Include(r => r.Staff)
                    .Include(r => r.Province);
            }
            return query;
        }

        public async Task<IEnumerable<Staff_region_rule>> GetAllAsync(bool includeDetails = true)
        {
            return await GetBaseQuery(includeDetails)
                .OrderBy(r => r.Province_id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Staff_region_rule>> GetByStaffAsync(int staffId)
        {
            return await GetBaseQuery(true)
                .Where(r => r.Staff_id == staffId)
                .OrderBy(r => r.Province_id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Staff_region_rule>> GetByProvinceAsync(int provinceId)
        {
            return await GetBaseQuery(true)
                .Where(r => r.Province_id == provinceId)
                .ToListAsync();
        }

        public async Task<Staff_region_rule?> GetByIdAsync(int id)
        {
            return await GetBaseQuery(true).FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Staff_region_rule?> FindByProvinceAndWardAsync(int provinceId, int? wardId)
        {
            var baseQuery = GetBaseQuery()
                .Where(r => r.Province_id == provinceId);

            if (!wardId.HasValue)
                return await baseQuery
                    .Where(r => r.Ward_ids == null || r.Ward_ids.Count == 0)
                    .FirstOrDefaultAsync();

            // Ưu tiên: rule có chứa đúng ward này
            var exact = await baseQuery
                .Where(r => r.Ward_ids != null && r.Ward_ids.Contains(wardId.Value))
                .FirstOrDefaultAsync();
            if (exact != null) return exact;

            // Fallback: rule toàn tỉnh
            return await baseQuery
                .Where(r => r.Ward_ids == null || r.Ward_ids.Count == 0)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsByStaffAndProvinceAsync(int staffId, int provinceId, int? excludeId = null)
        {
            var query = GetBaseQuery().Where(r => r.Staff_id == staffId && r.Province_id == provinceId);
            if (excludeId.HasValue)
                query = query.Where(r => r.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Staff_region_rule> AddAsync(Staff_region_rule rule)
        {
            rule.Created_at = DateTime.Now;
            rule.Updated_at = DateTime.Now;
            await _context.StaffRegionRules.AddAsync(rule);
            await _context.SaveChangesAsync();
            return rule;
        }

        public async Task UpdateAsync(Staff_region_rule rule)
        {
            rule.Updated_at = DateTime.Now;
            _context.StaffRegionRules.Update(rule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var rule = await _context.StaffRegionRules.FindAsync(id);
            if (rule != null)
            {
                rule.Is_deleted = true;
                rule.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
